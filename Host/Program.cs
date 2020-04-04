using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Host
{
    class Program
    {

        private static Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static void Main(string[] args)
        {
            Console.Title = "Client";
            LoopConnect();
            SendLoop();
            Console.ReadLine();
        }


        private static void SendLoop()
        {
            while (true)
            {
                Console.Write("Enter a request: ");
                string req = Console.ReadLine();
                byte[] buffer = Encoding.ASCII.GetBytes(req);
                _clientSocket.Send(buffer);

                //byte[] receivedBuf = new byte[1];
                //int rec = _clientSocket.Receive(receivedBuf);
                //byte[] data = new byte[rec];
                //Array.Copy(receivedBuf, data, rec);
                //Console.WriteLine("Received: " + Encoding.ASCII.GetString(data));
            }
        }
        private static void LoopConnect()
        {
            int attempts = 0;

            while (!_clientSocket.Connected)
            {
                try
                {
                    attempts++;
                    _clientSocket.Connect(IPAddress.Loopback, 100);
                    
                }
                catch (SocketException)
                {
                    Console.Clear();
                    Console.WriteLine("Connect attempts: " + attempts.ToString());
                    Console.WriteLine("IPAddress.Loopback: " + IPAddress.Loopback);
                }
            }
            Console.Clear();
            Console.WriteLine("Connected");
        }
    }
}
