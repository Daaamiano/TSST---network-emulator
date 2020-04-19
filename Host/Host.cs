using DataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;


namespace Host
{
    public class ObjectState
    {
        public Socket workSocket = null;
        public const int bufferSize = 1024;
        public byte[] buffer = new byte[bufferSize];
        public StringBuilder sb = new StringBuilder();
    }

    class Host
    {
        private static string hostName;
        private static int hostPort;
        private static IPAddress destAddress;
        private static IPAddress ipSourceAddress;
        private static int h1Port;
        private static IPAddress ipAddressH1;
        private static int h2Port;
        private static IPAddress ipAddressH2;
        private static int h3Port;
        private static IPAddress ipAddressH3;
        private static int h4Port;
        private static IPAddress ipAddressH4;
        private static int cableCloudPort;
        private static int destinationPort;
        private static IPAddress cableCloudIpAddress;
        private static ManualResetEvent allDone = new ManualResetEvent(false);
        private static string response = String.Empty;

        public Host(string filePath)
        {
            LoadPropertiesFromFile(filePath);
            Console.Title = hostName;
            StartHost();
        }

        public static void StartHost()
        {
            IPEndPoint localEndPoint = new IPEndPoint(cableCloudIpAddress, hostPort);
            Socket recSocket = new Socket(cableCloudIpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                recSocket.Bind(localEndPoint);
                recSocket.Listen(100);
                Console.WriteLine("Waiting for a incomming connection...");
                recSocket.BeginAccept(new AsyncCallback(AcceptCallback), recSocket);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }


            while (true)
            {
                Console.WriteLine($"You are host {hostName}");
                Console.WriteLine("Write '1'  if you want to send the message to another host ");

                int decision = int.Parse(Console.ReadLine());
                if (decision == 1)
                {
                    try
                    {
                        Socket hostSocket = new Socket(cableCloudIpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        hostSocket.BeginConnect(new IPEndPoint(cableCloudIpAddress, cableCloudPort),
                            new AsyncCallback(ConnectionCallBack), hostSocket);

                        Console.WriteLine("Choose which host you want to send the package to");
                        Console.WriteLine(@"To do this, write 'H1' or 'H2' or 'H3' or 'H4' -> host 1 is 'H1' etc.");
                        string nameDestinationHost = Console.ReadLine();
                        if (nameDestinationHost == "H1") 
                        {
                            destinationPort = h1Port;
                            destAddress = ipAddressH1;
                        }
                        else if (nameDestinationHost == "H2")
                        {
                            destinationPort = h2Port;
                            destAddress = ipAddressH2;
                        }
                        else if (nameDestinationHost == "H3")
                        {
                            destinationPort = h3Port;
                            destAddress = ipAddressH3;
                        }
                        else if (nameDestinationHost == "H4")
                        {
                            destinationPort = h4Port;
                            destAddress = ipAddressH4;
                        }

                        Send(hostSocket, $"Test taki o se {DateTime.Now} port docelowy to {destinationPort}");
                        allDone.WaitOne();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                else
                {
                    Console.WriteLine("You wrote something other than '1' ");
                    Console.WriteLine("Please try again: ");
                }
            }

        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            allDone.Set();

            Socket cloudSocketListener = (Socket)ar.AsyncState;
            Socket cloudSocketHandler = cloudSocketListener.EndAccept(ar);

            ObjectState state = new ObjectState();
            state.workSocket = cloudSocketHandler;
            cloudSocketHandler.BeginReceive(state.buffer, 0, ObjectState.bufferSize, 0, 
                new AsyncCallback(ReadCallback), state);
        }

        private static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            ObjectState state = (ObjectState)ar.AsyncState;
            Socket handler = state.workSocket;

            int read = handler.EndReceive(ar);

            if (read > 0)
            {
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, read));

                content = state.sb.ToString();
                Console.WriteLine(content);
                Package package = DeserializeFromJson(content);
            }
        }

        private void LoadPropertiesFromFile(string filePath)
        {
            var properties = new Dictionary<string, string>();
            foreach (var row in File.ReadAllLines(filePath))
            {
                properties.Add(row.Split('=')[0], row.Split('=')[1]);
            }
            hostName = properties["HOSTNAME"];
            hostPort = int.Parse(properties["HOSTPORT"]);
            ipSourceAddress = IPAddress.Parse(properties["IPSOURCEADDRESS"]);
            cableCloudIpAddress = IPAddress.Parse(properties["CABLECLOUDIPADDRESS"]);
            cableCloudPort = int.Parse(properties["CABLECLOUDPORT"]);

            if (hostName == "H1")
            {
                h2Port = int.Parse(properties["H2PORT"]);
                ipAddressH2 = IPAddress.Parse(properties["IPADDRESSH2"]);
                h3Port = int.Parse(properties["H3PORT"]);
                ipAddressH3 = IPAddress.Parse(properties["IPADDRESSH3"]);
                h4Port = int.Parse(properties["H4PORT"]);
                ipAddressH4 = IPAddress.Parse(properties["IPADDRESSH4"]);
            }
            else if (hostName == "H2")
            {
                h1Port = int.Parse(properties["H1PORT"]);
                ipAddressH1 = IPAddress.Parse(properties["IPADDRESSH1"]);
                h3Port = int.Parse(properties["H3PORT"]);
                ipAddressH3 = IPAddress.Parse(properties["IPADDRESSH3"]);
                h4Port = int.Parse(properties["H4PORT"]);
                ipAddressH4 = IPAddress.Parse(properties["IPADDRESSH4"]);
            }
            else if (hostName == "H3")
            {
                h1Port = int.Parse(properties["H1PORT"]);
                ipAddressH1 = IPAddress.Parse(properties["IPADDRESSH1"]);
                h2Port = int.Parse(properties["H2PORT"]);
                ipAddressH2 = IPAddress.Parse(properties["IPADDRESSH2"]);
                h4Port = int.Parse(properties["H4PORT"]);
                ipAddressH4 = IPAddress.Parse(properties["IPADDRESSH4"]);
            }
            else if (hostName == "H4")
            {
                h1Port = int.Parse(properties["H1PORT"]);
                ipAddressH1 = IPAddress.Parse(properties["IPADDRESSH1"]);
                h2Port = int.Parse(properties["H2PORT"]);
                ipAddressH2 = IPAddress.Parse(properties["IPADDRESSH2"]);
                h3Port = int.Parse(properties["H3PORT"]);
                ipAddressH3 = IPAddress.Parse(properties["IPADDRESSH3"]);
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
            ObjectState state = (ObjectState)ar.AsyncState;
            var hostSocket = state.workSocket;
            int byteRead = hostSocket.EndReceive(ar);
            if (byteRead > 0)
            {
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, byteRead));

                String content = String.Empty;
                content = state.sb.ToString();
                Package package = DeserializeFromJson(content);
                Console.WriteLine(package.message);

                hostSocket.BeginReceive(state.buffer, 0, ObjectState.bufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            else
            {
                if (state.sb.Length > 1)
                {
                    response = state.sb.ToString();
                }

                allDone.Set();
            }
        }

        private static void Send(Socket hostSocket, string data)
        {
            //public Package(int sourcePort, string destAddress, int destPort, string message)
            Package package = new Package(hostPort, destAddress.ToString(), destinationPort, data);
            string json = SerializeToJson(package);
            Console.WriteLine(json);
            byte[] byteData = Encoding.ASCII.GetBytes(json);
            hostSocket.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), hostSocket);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket hostSocket = (Socket)ar.AsyncState;
                int byteSent = hostSocket.EndSend(ar);
                Console.WriteLine($"Sent: {byteSent} bytes to Cable Cloud");
                allDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static string SerializeToJson(Package package)
        {
            string jsonString;
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            jsonString = JsonSerializer.Serialize(package, options);

            return jsonString;
        }

        public static Package DeserializeFromJson(string serializedString)
        {
            Package package = new Package();
            package = JsonSerializer.Deserialize<Package>(serializedString);
            return package;
        }

        private static void ConnectionCallBack(IAsyncResult ar)
        {
            try
            {
                Socket hostSocket = (Socket)ar.AsyncState;
                hostSocket.EndConnect(ar);
                Console.WriteLine("Host connected to cable cloud");
                allDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

}