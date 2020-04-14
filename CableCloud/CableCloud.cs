using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
        


        private IPAddress Address = IPAddress.Parse("127.0.0.1"); // jedno IP do wszystkiego
        private const int cableCloudPort = 5001;
        private int routerPort = 5000;

        private static ManualResetEvent connectCompleted = new ManualResetEvent(false);
        private static ManualResetEvent sendCompleted = new ManualResetEvent(false);
        private static ManualResetEvent receiveCompleted = new ManualResetEvent(false);
        private static string response = String.Empty;


        public CableCloud()
        {
            //przypomniec sobie czy potrzebuje wczytac tablice i czy przeciążyć konstruktor
            Start();
        }

        public void Start()
        {
            Socket cloudSocket = new Socket(Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            cloudSocket.BeginConnect(new IPEndPoint(Address, cableCloudPort),
                new AsyncCallback(ConnectionCallBack), cloudSocket);

            try
            {
                Receive(cloudSocket);
                receiveCompleted.WaitOne();
                Console.WriteLine($"Response received {response}");
                cloudSocket.Shutdown(SocketShutdown.Both);
                cloudSocket.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        private static void Send(Socket s, String d)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(d);
            s.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), s);
        }

        private static void Receive(Socket cloudSocket)
        {
            try
            {
                StateObject state = new StateObject();
                state.workSocket = cloudSocket;
                cloudSocket.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        private static void ReceiveCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            var hostSocket = state.workSocket;
            int byteRead = hostSocket.EndReceive(ar);
            if (byteRead > 0)
            {
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, byteRead));
                hostSocket.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            else
            {
                if (state.sb.Length > 1)
                {
                    response = state.sb.ToString();
                }

                receiveCompleted.Set();
            }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket cloudSocket = (Socket)ar.AsyncState;
                int byteSent = cloudSocket.EndSend(ar);
                Console.WriteLine($"Sent: {byteSent} bytes to Cable Cloud");
                sendCompleted.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void ConnectionCallBack(IAsyncResult ar)
        {
            try
            {
                Socket cloudSocket = (Socket)ar.AsyncState;
                cloudSocket.EndConnect(ar);
                Console.WriteLine("Host connected to cable cloud");
                connectCompleted.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

    }
}
