using DataStructures;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace CableCloud
{
    public class StateObject
    {
        public Socket workSocket = null;
        public const int bufferSize = 1024;
        public byte[] buffer = new byte[bufferSize];
        public StringBuilder sb = new StringBuilder();
    }

    class CableCloud
    {


        private static ManualResetEvent done = new ManualResetEvent(false);


        public CableCloud()
        {

        }

        public void Start(int myPort)
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress address = IPAddress.Parse("127.0.0.1");

            Console.WriteLine("port number is " + myPort);
            IPEndPoint localEndPoint = new IPEndPoint(address, myPort);

            Socket cloudSocket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                cloudSocket.Bind(localEndPoint);
                cloudSocket.Listen(100);

                while (true)
                {
                    done.Reset();
                    Console.WriteLine("Waiting for a incomming connection...");
                    cloudSocket.BeginAccept(new AsyncCallback(AcceptCallback), cloudSocket);
                    done.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("Press any kay to cont..");
            Console.Read();

        }

        private void AcceptCallback(IAsyncResult ar)
        {
            done.Set();

            Socket cloudSocketListener = (Socket)ar.AsyncState;
            Socket cloudSocketHandler = cloudSocketListener.EndAccept(ar);

            StateObject state = new StateObject();
            state.workSocket = cloudSocketHandler;
            cloudSocketHandler.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        private void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            int read = handler.EndReceive(ar);

            if (read > 0)
            {
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, read));

                content = state.sb.ToString();

                Console.WriteLine(content);
                Package package = DeserializeFromJson(content);

                Console.WriteLine("wysylam na port");
                Console.WriteLine(package.incomingPort);
                
                // przesylanie wiadomosci na nowy port  trzeba 3x odpalic VS host, chmura do przeslania, chmura do odbioru (zamiast routera)               * 
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                Console.WriteLine("write port number to resend the message");
                string port = Console.ReadLine();
                int result = Int32.Parse(port);
                IPAddress address = IPAddress.Parse("127.0.0.1");
                Socket sendSocket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                state.workSocket = sendSocket;
                sendSocket.BeginConnect(new IPEndPoint(address, result),
                new AsyncCallback(ConnectionCallBack), sendSocket);
                

                if (true)
                {
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}", content.Length, content);

                    // jak przesylanie wiadomosci dalej to handler zamienic na sendSocket
                    Send(sendSocket, content); // to jak przesylanie wiadomosci na nowy port
                    //Send(handler, content); // to jak chcemy wyslac echo
                }
                else
                {
                    handler.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
            }
        }

        private void Send(Socket handler, string content)
        {
            byte[] data = Encoding.ASCII.GetBytes(content);

            handler.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        private static void ConnectionCallBack(IAsyncResult ar)
        {
            try
            {
                Socket hostSocket = (Socket)ar.AsyncState;
                hostSocket.EndConnect(ar);
                Console.WriteLine("Host connected to cable cloud");
                done.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;

                int sent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client", sent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public string SerializeToJson(Package package)
        {
            string jsonString;
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            jsonString = JsonSerializer.Serialize(package, options);

            return jsonString;
        }

        public Package DeserializeFromJson(string serializedString)
        {
            Package package = new Package();
            package = JsonSerializer.Deserialize<Package>(serializedString);
            return package;
        }
    }
}
