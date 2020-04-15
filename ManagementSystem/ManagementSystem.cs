using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using DataStructures;

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
        }

        public void Start()
        {
            //msSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            ListenForConnections();
        }

        private void ListenForConnections()
        {
            Console.WriteLine("Awaiting connection...");
            msSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
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
            if (content == "HELLO")
            {
                // Send response message.
                Console.WriteLine("Received HELLO.");
                SendResponse(handler, content);
            }
            else
            {
                Console.WriteLine($"Message received: {content}");
                SendMessage(handler);
            }
            state.sb.Clear();
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        private void SendResponse(Socket handler, String data)
        {
            //byte[] byteData = Encoding.ASCII.GetBytes(data);
            byte[] responseMessage = Encoding.ASCII.GetBytes(data);
            handler.Send(responseMessage);
            SendMessage(handler);
            //handler.BeginSend(byteData, 0, byteData.Length, 0,
            //new AsyncCallback(SendCallback), handler);
        }

        private void SendMessage(Socket handler)
        {
            
                try
                {
                    Console.WriteLine("Type a message: ");
                    string message = Console.ReadLine();

                    List<int> testLabels = new List<int> { 1, 2, 3, 4 };
                    Package package = new Package(ipAddress.ToString(), port, testLabels, message);
                    string json = SerializeToJson(package);

                    byte[] byteData = Encoding.ASCII.GetBytes(json);
                    //handler.BeginSend(byteData, 0, byteData.Length, 0,
                    //   new AsyncCallback(SendCallback), handler);
                    handler.Send(byteData);
                }
                catch
                {
                    Console.WriteLine("Connection with Routerrro is booomerrrro");
                    // msSocket.Shutdown(SocketShutdown.Both);
                    //ListenForConnections();
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
                SendMessage(handler);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }
}
