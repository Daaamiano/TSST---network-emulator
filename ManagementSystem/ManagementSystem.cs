using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ManagementSystem
{
    class StateObject
    {
        public Socket workSocket = null;
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
    }

    class ManagementSystem
    {
        private Socket msSocket;
        private int port = 5000;
        private IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        private ManualResetEvent allDone = new ManualResetEvent(false);

        public ManagementSystem()
        {
            Console.Title = "ManagementSystem";
            //Console.ReadLine();
        }

        public void Start()
        {
            msSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            ListenForConnections();
        }

        private void ListenForConnections()
        {
            Console.WriteLine("Awaiting connection...");
            try
            {
                msSocket.Bind(new IPEndPoint(ipAddress, port));
                msSocket.Listen(100);
                while (true)
                {
                    allDone.Reset();
                    msSocket.BeginAccept(new AsyncCallback(AcceptCallback), msSocket);
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("Closing the listener...");
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            allDone.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.  
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        private void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = null;
            handler = state.workSocket;

            if (handler != null)
            {
                Console.WriteLine("Socket connected.");
            }

            // Read data from the client socket.
            int bytesRead = handler.EndReceive(ar);
            if (bytesRead > 0)
            {
                Console.WriteLine("Data read from the client.");
            }

            state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
            var content = state.sb.ToString();
            Console.WriteLine($"Message received: {content}");

            // Send response message.
            SendResponse(handler, content);

        }

        private void SendResponse(Socket handler, String data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            //byte[] responseMessage = Encoding.ASCII.GetBytes("HELLO");
            //handler.Send(responseMessage);
            handler.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), handler);
        }

        private void SendMessege(Socket handler)
        {


            Console.WriteLine("Type a message: ");
            string message = Console.ReadLine();

            byte[] byteData = Encoding.ASCII.GetBytes(message);
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);


        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);
                //handler.Shutdown(SocketShutdown.Both);
                // handler.Close();
                SendMessege(handler);


            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }
}
