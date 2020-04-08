using System;
using System.Collections.Generic;
using System.Text;

namespace ManagementSystem
{
    class ManagementSystem
    {
        private Socket managmentSystemSocket;
        private static readonly byte[] buffer = new byte[BUFFER_SIZE];

        public ManagementSystem()
        {

        }

        private void ListenForConnections()
        {
            Console.WriteLine("Setting up MS...");
            managmentSystemSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            managmentSystemSocket.Bind(new IPEndPoint.Any, 5000);
            managmentSystemSocket.Listen(0);
            managmentSystemSocket.BeginAccept(AcceptCallback, null);
            Console.WriteLine("MS setup complete");

        }
        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket;

            try
            {
                socket = serverSocket.EndAccept(AR);
            }
            catch (ObjectDisposedException) 
            {
                return;
            }

            managmentSystemSocket.Add(socket);
            socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
            Console.WriteLine("Client connected, waiting for request...");
            managmentSystemSocket.BeginAccept(AcceptCallback, null);
        }
    }
}
