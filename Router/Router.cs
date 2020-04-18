using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DataStructures;

namespace Router
{
    class Router
    {
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
            cableCloudSocket = new Socket(cloudAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            ConnectToManagementSystem();
        }

        private void ConnectToManagementSystem()
        {
            Console.WriteLine("Connecting to management system...");
            while (true)
            {
                managementSystemSocket = new Socket(managementSystemAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    managementSystemSocket.Connect(new IPEndPoint(managementSystemAddress, managementSystemPort));
                   
                }
                catch (Exception)
                {
                    Console.WriteLine("Couldn't connect to management system.");
                    Console.WriteLine("Reconnecting...");
                    Thread.Sleep(5000);
                    continue;
                }

                try
                {
                    Console.WriteLine("Sending CONNECTED to management system...");
                    string connectedMessage = "CONNECTED";
                    Package helloPackage = new Package(routerName, managementSystemAddress.ToString(), managementSystemPort, connectedMessage);
                    managementSystemSocket.Send(Encoding.ASCII.GetBytes(SerializeToJson(helloPackage)));

                    byte[] buffer = new byte[256];
                    int bytes = managementSystemSocket.Receive(buffer);

                    var message = Encoding.ASCII.GetString(buffer, 0, bytes);

                    if (message.Contains("CONNECTED"))
                    {
                        Console.WriteLine("Connected to management system.");
                        ReceiveMessages();
                        break;
                    }
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e.Source);
                    Console.WriteLine("Couldn't send hello to management system.");
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
                Console.WriteLine("Received from management system: {0}", message);
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

        public bool Route()
        {
            return true;
        }
    }
}