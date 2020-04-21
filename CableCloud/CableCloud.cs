using DataStructures;
using System;
using System.Collections.Generic;
using System.IO;
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

    class Tunnel
    {
        
        public string incomingObject;
        public string destinationObject;
        public int destinationPort;
        public Tunnel(string incomingObject, int destinationPort, string destinationObject)
        {
            this.incomingObject = incomingObject;
            this.destinationObject = destinationObject;
            this.destinationPort = destinationPort;

        }
    }

    class CableCloud
    {

        private Dictionary<string, Socket> connectedSockets = new Dictionary<string, Socket>();
        private static ManualResetEvent done = new ManualResetEvent(false);
        Dictionary<int, Tunnel> tunnels = new Dictionary<int, Tunnel>();


        public CableCloud(string configFilePath)
        {
            LoadTunnels(configFilePath);
            Start(5001);
            Console.ReadLine();
        }

        public void Start(int myPort)
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress address = IPAddress.Parse("127.0.0.1");

            IPEndPoint localEndPoint = new IPEndPoint(address, myPort);

            Socket cloudSocket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                cloudSocket.Bind(localEndPoint);
                cloudSocket.Listen(100);

                while (true)
                {
                    done.Reset();
                    Logs.ShowLog(LogType.INFO, "Waiting for a incomming connection...");                    
                    cloudSocket.BeginAccept(new AsyncCallback(AcceptCallback), cloudSocket);

                    /*
                    StateObject state = new StateObject();
                    state.workSocket = cloudSocket;

                    cloudSocket.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0, new AsyncCallback(ReadCallback), state);
                    */

                    
                    done.WaitOne();
                }

            }
            catch (Exception e)
            {
                Logs.ShowLog(LogType.ERROR, e.ToString());
            }
            //Console.WriteLine("Press any kay to cont..");
            ///Console.Read();

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
            //String content = String.Empty;

            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = null;
            handler = state.workSocket;
            Package package = new Package();

            try
            {
                int read = handler.EndReceive(ar);
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, read));

                var content = state.sb.ToString();

                //Console.WriteLine(content);
                Logs.ShowLog(LogType.INFO, ("Read {" + content.Length.ToString() + "} bytes from socket. \n Data : " + content));
                package = DeserializeFromJson(content);

                //slownik sucketow

                if (package.message == "CONNECTED")
                {
                    // Send response message.
                    Logs.ShowLog(LogType.CONNECTED, $"Connection with {package.sourceName} established.");
                    try
                    {
                        connectedSockets.Add(package.sourceName, handler);

                        Console.WriteLine("dodano do tablicy socket: " + package.sourceName);

                        //

                        //state.workSocket = handler;
                        //handler.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0, new AsyncCallback(ReadCallback), state);
                    }
                    catch
                    {
                        // Skip adding if such key already exists -> the router is reconnecting.
                    }
                    Send(handler, content);
                    //handler.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
                else
                {
                    //tunelowanie test
                    Console.WriteLine("incoming port");
                    Console.WriteLine(package.incomingPort);


                    //tunelowanie test 
                    Console.WriteLine("destination port tunelu");
                    Console.WriteLine(tunnels[package.incomingPort].destinationPort);

                    //tunelowanie test 
                    Console.WriteLine("new incoming port");
                    Console.WriteLine(tunnels[package.incomingPort].destinationPort);
                    package.incomingPort = tunnels[package.incomingPort].destinationPort;

                    //serializacja pakietu z nowym incoming port
                    content = SerializeToJson(package);

                    /*
                    // przesylanie wiadomosci na nowy port  trzeba 3x odpalic VS host, chmura do przeslania, chmura do odbioru (zamiast routera)               * 
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                    Console.WriteLine("write port number to resend the message (new incomiing port)");
                    string port = Console.ReadLine();
                    int result = Int32.Parse(port);
                    IPAddress address = IPAddress.Parse("127.0.0.1");
                    Socket sendSocket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    state.workSocket = sendSocket;
                    sendSocket.BeginConnect(new IPEndPoint(address, result),
                    //sendSocket.BeginConnect(new IPEndPoint(address, tunnels[].destinationPort),
                    new AsyncCallback(ConnectionCallBack), sendSocket);
                    */


                    Logs.ShowLog(LogType.INFO, ("Sent {" + content.Length.ToString() + "} bytes to client. \n Data : "+ content));


                    // jak przesylanie wiadomosci dalej to handler zamienic na sendSocket
                    //Send(connectedSockets[package.sourceName], content); // to jak przesylanie wiadomosci na nowy port
                    Send(handler, content); // to jak chcemy wyslac echo


                }
                state.sb.Clear();
                handler.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0, new AsyncCallback(ReadCallback), state);

                /*
                //tunelowanie test
                Console.WriteLine("incoming port");
                Console.WriteLine(package.incomingPort);

               
                //tunelowanie test 
                Console.WriteLine("destination port tunelu");
                Console.WriteLine(tunnels[package.incomingPort].destinationPort);

                //tunelowanie test 
                Console.WriteLine("new incoming port");
                Console.WriteLine(tunnels[package.incomingPort].destinationPort);
                package.incomingPort = tunnels[package.incomingPort].destinationPort;

                //serializacja pakietu z nowym incoming port
                content = SerializeToJson(package);

                // przesylanie wiadomosci na nowy port  trzeba 3x odpalic VS host, chmura do przeslania, chmura do odbioru (zamiast routera)               * 
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                Console.WriteLine("write port number to resend the message (new incomiing port)");
                string port = Console.ReadLine();
                int result = Int32.Parse(port);
                IPAddress address = IPAddress.Parse("127.0.0.1");
                Socket sendSocket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                state.workSocket = sendSocket;
                sendSocket.BeginConnect(new IPEndPoint(address, result),
                //sendSocket.BeginConnect(new IPEndPoint(address, tunnels[].destinationPort),
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
                */
            }            
            catch(Exception e)
            {
                //var exceptionTrace = new StackTrace(e).GetFrame(0).GetMethod().Name;
                //Console.WriteLine(exceptionTrace);
                Logs.ShowLog(LogType.ERROR, "Connection with router lost." );
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
}

        private void LoadTunnels(string configFilePath)
        {
            foreach (var row in File.ReadAllLines(configFilePath))
            {

                var splitRow = row.Split(", ");
                if (splitRow[0] =="#X" )
                {
                    continue;
                }
               //Console.WriteLine(splitRow[0]);
                //Console.Read();
                //Console.WriteLine(splitRow[0] + " " + splitRow[1] + " " + int.Parse(splitRow[2]) + " " + splitRow[3]);
                tunnels.Add(int.Parse(splitRow[0]), new Tunnel(splitRow[1], int.Parse(splitRow[2]), splitRow[3]));
            }            
        }

        private void Send(Socket handler, string content)
        {
            byte[] data = Encoding.ASCII.GetBytes(content);

            handler.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        /*
        private static void ConnectionCallBack(IAsyncResult ar)
        {
            try
            {
                Socket hostSocket = (Socket)ar.AsyncState;
                hostSocket.EndConnect(ar);
                Logs.ShowLog(LogType.INFO, "Host connected to cable cloud");
                done.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        */

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;

                int sent = handler.EndSend(ar);
                Logs.ShowLog(LogType.INFO, "Sent {" + sent.ToString() +"} bytes to client");

               
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
