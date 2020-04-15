using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using DataStructures;

namespace Router
{
    class StateObject
    {
        public Socket workSocket = null;
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
    }

    class Router
    {
        private byte[] buffer = new byte[2048];
        private Socket managementSystemSocket;
        private Socket cableCloudSocket;

        public string routerName;
        private IPAddress ipAddress;        // adres IP danego routera
        private IPAddress cloudAddress;
        private int cloudPort;
        private IPAddress managementSystemAddress;
        private int managementSystemPort;

        private Socket cloudSocket;
        private MplsFibTable mplsFibTable;
        private IpFibTable ipFibTable;
        private IlmTable ilmTable;
        private FtnTable ftnTable;
        private NhlfeTable nhlfeTable;

        private ManualResetEvent connectDone = new ManualResetEvent(false);

        public Router(string routerConfigFilePath, string tablesConfigFilePath)
        {
            LoadPropertiesFromFile(routerConfigFilePath);
            LoadTablesFromFile(tablesConfigFilePath);
            Console.Title = "Router";
            //test
            /*
            List<int> testLabels = new List<int> { 1, 2, 3, 4 };
            Package testPackage = new Package(managementSystemAddress.ToString(), managementSystemPort, testLabels, "Dupcia Damiana");
            string json = SerializeToJson(testPackage);
            testPackage = DeserializeFromJson(json);
            */
            //Console.ReadLine();
        }

        public void Start()
        {
            managementSystemSocket = new Socket(managementSystemAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            cableCloudSocket = new Socket(cloudAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            ConnectToManagementSystem();
        }

        private void ConnectToManagementSystem()
        {
            Console.WriteLine("Connecting to management system...");
            while (true)
            {
                //managementSystemSocket.ReceiveTimeout = 20000;

                try
                {
                    var result = managementSystemSocket.BeginConnect(new IPEndPoint(managementSystemAddress, managementSystemPort), null, null);

                    bool success = result.AsyncWaitHandle.WaitOne(5000, true);
                    if (success)
                    {
                        managementSystemSocket.EndConnect(result);
                    }
                    else
                    {
                        managementSystemSocket.Close();
                        Console.WriteLine("Connection to MS not established - timeout...");
                        continue;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Retrying...");
                }

                try
                {
                    Console.WriteLine("Sending hello to MS...");
                    managementSystemSocket.Send(Encoding.ASCII.GetBytes("HELLO"));

                    byte[] buffer = new byte[256];
                    int bytes = managementSystemSocket.Receive(buffer);

                    var message = Encoding.ASCII.GetString(buffer, 0, bytes);

                    if (message.Contains("HELLO"))
                    {
                        Console.WriteLine("Estabilished connection with MS");
                        //Console.ReadLine();
                        ReceiveMessages();
                        break;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Couldn't connect to MS!");
                }
            }
        }

        private void ReceiveMessages()
        {
            while (true)
            {
                byte[] buffer = new byte[256];
                int bytes = managementSystemSocket.Receive(buffer);
                var message = Encoding.ASCII.GetString(buffer, 0, bytes);
                Console.WriteLine(message);
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.  
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void LoadPropertiesFromFile(string configFilePath)
        {
            var properties = new Dictionary<string, string>();
            foreach (var row in File.ReadAllLines(configFilePath))
            {
                properties.Add(row.Split('=')[0], row.Split('=')[1]);
            }
            routerName = properties["ROUTERNAME"];
            ipAddress = IPAddress.Parse(properties["IPADDRESS"]);
            managementSystemAddress = IPAddress.Parse(properties["MANAGEMENTSYSTEMADDRESS"]);
            cloudAddress = IPAddress.Parse(properties["CLOUDADDRESS"]);
            managementSystemPort = int.Parse(properties["MANAGEMENTSYSTEMPORT"]);
            cloudPort = int.Parse(properties["CLOUDPORT"]);
        }

        private void LoadTablesFromFile(string tablesFilePath)
        {
            // Router LSR nie potrzebuje wszystkich tablic. W naszej topologii tylko R2 jest LSR.
            if (routerName != "R2")
            {
                mplsFibTable = new MplsFibTable(tablesFilePath, routerName);
                ipFibTable = new IpFibTable(tablesFilePath, routerName);
                nhlfeTable = new NhlfeTable(tablesFilePath, routerName);
                ftnTable = new FtnTable(tablesFilePath, routerName);
                ilmTable = new IlmTable(tablesFilePath, routerName);
            }
            else
            {
                nhlfeTable = new NhlfeTable(tablesFilePath, routerName);
                ilmTable = new IlmTable(tablesFilePath, routerName);
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

        public void PushLabel(Package package, int label)
        {
            package.labels.Add(label);
        }

        public void PopLabel(Package package)
        {
            if (package.labels.Any())
            {
                package.labels.RemoveAt(package.labels.Count - 1);
            }
            else
            {
                Console.WriteLine("Próba zdjęcia etykiety z pustej listy etykiet.");
            }
        }

        public bool Send()
        {
            return true;
        }

        public bool Route()
        {
            return true;
        }
    }
}