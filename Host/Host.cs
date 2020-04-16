using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Text.Json;
using DataStructures;



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
        private string hostName;
        private int sourcePort;
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
        private static ManualResetEvent connectCompleted = new ManualResetEvent(false);
        private static ManualResetEvent sendCompleted = new ManualResetEvent(false);
        private static ManualResetEvent receiveCompleted = new ManualResetEvent(false);
        private static string response = String.Empty;
        private static string response = Strng.Empty;

        public Host(string filePath)
        {
            LoadPropertiesFromFile(filePath);
            Console.Title = hostName;
            StartHost();
        }

        public static void StartHost()
        {
            Socket hostSocket = new Socket(cableCloudIpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            hostSocket.BeginConnect(new IPEndPoint(cableCloudIpAddress, cableCloudPort),
            new AsyncCallback(ConnectionCallBack), hostSocket);


            bool correctChoice = false;
            while(true)
            {
                Console.WriteLine("You are host");
                Console.WriteLine("Write '1'  if you want to send the message to another host ");

                int decision = int.Parse(Console.ReadLine());
                if (decision == 1)
                {
                    try
                    {
                        correctChoice = true;
                        Console.WriteLine("Choose which host you want to send the package to");
                        Console.WriteLine(@"To do this, write 'H1' or 'H2' or 'H3' or 'H4' -> host 1 is 'H1' etc.");
                        string nameDestinationHost = Console.ReadLine();
                        if (nameDestinationHost == "H1")
                            destinationPort = h1Port;
                        else if (nameDestinationHost == "H2")
                            destinationPort = h2Port;
                        else if (nameDestinationHost == "H3")
                            destinationPort = h3Port;
                        else if (nameDestinationHost == "H4")
                            destinationPort = h4Port;

                        Send(hostSocket, $"Test taki o se {DateTime.Now} port docelowy to {destinationPort} <EOF>");
                        sendCompleted.WaitOne();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }        
                else
                {
                    correctChoice = false;
                    Console.WriteLine("You wrote something other than '1' ");
                    Console.WriteLine("Please try again: ");
                }

                Receive(hostSocket);
                receiveCompleted.WaitOne();
                Console.WriteLine($"Response received {response}");
                hostSocket.Shutdown(SocketShutdown.Both);
                hostSocket.Close();
            }
            
        }

        private void LoadPropertiesFromFile(string filePath)
        {
            string[] lines = System.IO.File.ReadAllLines(filePath);
            hostName = lines[1];
            sourcePort = int.Parse(lines[2]);
            ipSourceAddress = IPAddress.Parse(lines[3]);
            cableCloudIpAddress = IPAddress.Parse(lines[10]);
            cableCloudPort = int.Parse(lines[11]);

            if (hostName == "H1")
            {
                h2Port = int.Parse(lines[4]);
                ipAddressH2 = IPAddress.Parse(lines[5]);
                h3Port = int.Parse(lines[6]);
                ipAddressH3 = IPAddress.Parse(lines[7]);
                h4Port = int.Parse(lines[8]);
                ipAddressH4 = IPAddress.Parse(lines[9]);
            }
            else if (hostName == "H2")
            {
                h1Port = int.Parse(lines[4]);
                ipAddressH1 = IPAddress.Parse(lines[5]);
                h3Port = int.Parse(lines[6]);
                ipAddressH3 = IPAddress.Parse(lines[7]);
                h4Port = int.Parse(lines[8]);
                ipAddressH4 = IPAddress.Parse(lines[9]);
            }
            else if (hostName == "H3")
            {
                h1Port = int.Parse(lines[4]);
                ipAddressH1 = IPAddress.Parse(lines[5]);
                h2Port = int.Parse(lines[6]);
                ipAddressH2 = IPAddress.Parse(lines[7]);
                h4Port = int.Parse(lines[8]);
                ipAddressH4 = IPAddress.Parse(lines[9]);
            }
            else if (hostName == "H4")
            {
                h1Port = int.Parse(lines[4]);
                ipAddressH1 = IPAddress.Parse(lines[5]);
                h2Port = int.Parse(lines[6]);
                ipAddressH2 = IPAddress.Parse(lines[7]);
                h3Port = int.Parse(lines[8]);
                ipAddressH3 = IPAddress.Parse(lines[9]);
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
            //public Package(int sourcePort, string destAddress, int destPort, string message) 
            Package package = new Package(12345, "ziomki tomka", 2331341, "data");
            string jsonPackage = SerializeToJson(package);

            byte[] byteData = Encoding.ASCII.GetBytes(jsonPackage);
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

        private static void ConnectionCallBack(IAsyncResult ar)
        {
            try
            {
                Socket hostSocket = (Socket) ar.AsyncState;
                hostSocket.EndConnect(ar);
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

