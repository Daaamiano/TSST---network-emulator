using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ManagementSystem
{
    class ManagementSystem
    {
        private static Socket RouterSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public ManagementSystem()
        {
            Console.Title = "ManagmentSystem";
            LoopConnect();
            SendLoop();
            Console.ReadLine();
        }

        private static void SendLoop()
        {
            while (true)                        //pętla wysyłająca i odbierająca. Po wysłaniu od razu czeka na odpowiedź
            {
                Console.Write("Enter a request: ");
                string req = Console.ReadLine();
                byte[] buffer = Encoding.ASCII.GetBytes(req);
                RouterSocket.Send(buffer);

                byte[] receivedBuf = new byte[1024];
                int rec = RouterSocket.Receive(receivedBuf);
                byte[] data = new byte[rec];
                Array.Copy(receivedBuf, data, rec);
                Console.WriteLine("Received: " + Encoding.ASCII.GetString(data));
            }
        }
        private static void LoopConnect()           //pętla sprawdzająca połączenie pomiędzy MS a Routerem
        {
            int attempts = 0;

            while (!RouterSocket.Connected)
            {
                try
                {
                    attempts++;
                    RouterSocket.Connect(IPAddress.Loopback, 5000);
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
