using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace Host
{
    public class ObjectState
    {
        public Socket workSocket = null;
        public const int bufferSize = 256;
        public byte[] buffer = new byte[bufferSize];
        public StringBuilder sb = new StringBuilder();
    } 
    
    class Host
    {
        private const int cableCloudPort = 5001;
        private static IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        private static ManualResetEvent connectCompleted = new ManualResetEvent(false);
        private static ManualResetEvent sendCompleted = new ManualResetEvent(false);
        private static ManualResetEvent receiveCompleted = new ManualResetEvent(false);
        private static string response = String.Empty;

        public Host()
        {
            Console.Title = "Host";
            StartHost();
            Console.ReadLine();
        }

        public static void StartHost()
        {
            try
            {
                Socket hostSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                hostSocket.BeginConnect(new IPEndPoint(ipAddress, cableCloudPort), 
                    new AsyncCallback(ConnectionCallBack), hostSocket);
                Send(hostSocket, $"Test taki o se {DateTime.Now} <EOF>");
                sendCompleted.WaitOne();

                Receive(hostSocket);
                receiveCompleted.WaitOne();
                Console.WriteLine($"Response received {response}");
                hostSocket.Shutdown(SocketShutdown.Both);
                hostSocket.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void Receive(Socket hostSocket)
        {
            try 
            {
                ObjectState state = new ObjectState();
                state.workSocket = hostSocket;
                hostSocket.BeginReceive(state.buffer, 0, ObjectState.bufferSize, 0, 
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e) 
            {
                Console.WriteLine(e);
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            ObjectState state = (ObjectState) ar.AsyncState;
            var hostSocket = state.workSocket;
            int byteRead = hostSocket.EndReceive(ar);
            if (byteRead > 0)
            {
                state.sb.Append(Encoding.ASCII.GetString(state.buffer,0,byteRead));
                hostSocket.BeginReceive(state.buffer, 0, ObjectState.bufferSize, 0, 
                    new AsyncCallback(ReceiveCallback), state);
            }
            else
            {
                if(state.sb.Length > 1)
                {
                    response = state.sb.ToString();
                }
                
                receiveCompleted.Set();
            }
        }

        private static void Send(Socket hostSocket, string data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            hostSocket.BeginSend(byteData, 0, byteData.Length, 0, 
                new AsyncCallback(SendCallback), hostSocket);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket hostSocket = (Socket) ar.AsyncState;
                int byteSent = hostSocket.EndSend(ar);
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
                Socket hostSocket = (Socket) ar.AsyncState;
                hostSocket.EndConnect(ar);
                Console.WriteLine($"Socket connection : {hostSocket.RemoteEndPoint.ToString()}");
                connectCompleted.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
    
}

// w jaki sposob pakujemy pakiet? - json? czy string z portem docelowym i wiadomoscia oddzielny np. srednikiem - potem cablecloud sobie odczyta to
// SocketFlags - moze do tworzenia oakietu (rozne ifnromacje tam schowac) ????
// The program '[19332] Host.dll' has exited with code -1 (0xffffffff). - o co chodzi???
// czy .net core 3.1 jest na pewno git? - czy zabiera jakies funkcje; czy nie lepsze framework jak komandos sugerowal ?