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
            Console.Read();
            SendLoop();
        }

        private static void SendLoop()
        {
            while (true)
            {
                Console.Write("Enter a request: ");
                string req = Console.ReadLine();
                byte[] buffer = Encoding.ASCII.GetBytes(req);
                _clientSocket.Send(buffer);

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
                }
            }
            Console.Clear();
            Console.WriteLine("Connected");
        }
    }

}
