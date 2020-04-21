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

        private MplsFibTable mplsFibTable;
        private IpFibTable ipFibTable;
        private IlmTable ilmTable;
        private FtnTable ftnTable;
        private NhlfeTable nhlfeTable;

        private string routerTablesFilePath;

        public Router(string routerConfigFilePath, string tablesConfigFilePath)
        {
            routerTablesFilePath = tablesConfigFilePath;
            LoadPropertiesFromFile(routerConfigFilePath);
            LoadTablesFromFile(tablesConfigFilePath);
            Console.Title = $"{routerName}";
        }

        public void Start()
        {
            ConnectToManagementSystem();
            //var t = Task.Run(action: () => ConnectToManagementSystem());
            //ConnectToCloud();
            Console.ReadLine();
        }

        private void ConnectToCloud()
        {
            while (true)
            {
                Console.WriteLine("Connecting to cable cloud...");
                cableCloudSocket = new Socket(cloudAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    cableCloudSocket.Connect(new IPEndPoint(cloudAddress, cloudPort));

                }
                catch (Exception)
                {
                    Console.WriteLine("Couldn't connect to cable cloud.");
                    Console.WriteLine("Retrying...");
                    Thread.Sleep(5000);
                    continue;
                }

                try
                {
                    /*while (true)
                    {
                        handleMessageFromCloud();
                        
                    }*/
                    Package testpackage = new Package(1010, cloudAddress.ToString(), cloudPort, "TESTOWA WIADOMOŚĆ");
                    SendPackageToCloud(testpackage);
                    break;
                }
                catch (Exception)
                {
                    //Console.WriteLine(e.Source);
                    Console.WriteLine("Connection to cable cloud lost.");
                }
            }
        }

        private void ConnectToManagementSystem()
        {
            while (true)
            {
                Console.WriteLine("Connecting to management system...");
                managementSystemSocket = new Socket(managementSystemAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    managementSystemSocket.Connect(new IPEndPoint(managementSystemAddress, managementSystemPort));
                   
                }
                catch (Exception)
                {
                    Console.WriteLine("Couldn't connect to management system.");
                    Console.WriteLine("Retrying...");
                    Thread.Sleep(5000);
                    continue;
                }

                try
                {
                    Console.WriteLine("Sending CONNECTED to management system...");
                    string connectedMessage = "CONNECTED";
                    Package connectedCheckPackage = new Package(routerName, managementSystemAddress.ToString(), managementSystemPort, connectedMessage);
                    managementSystemSocket.Send(Encoding.ASCII.GetBytes(SerializeToJson(connectedCheckPackage)));
                    while (true)
                    {
                        handleResponseFromMS();
                    }
                }
                catch (Exception)
                {
                    //Console.WriteLine(e.Source);
                    Console.WriteLine("Connection to management system lost.");
                }
            }
        }

        private void handleMessageFromCloud()
        {
            try
            {
                Package receivedPackage = ReceiveMessage();
                Route(receivedPackage);
                SendPackageToCloud(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("Couldn't perform routing for received package.");
            }
        }

        private void handleResponseFromMS()
        {
            Package receivedPackage = ReceiveMessage();

            if (receivedPackage.message.Contains("CONNECTED"))
            {
                Console.WriteLine("Connected to management system.");
            }
            else if (receivedPackage.message.Contains("RELOAD TABLES"))
            {
                Console.WriteLine(SerializeToJson(receivedPackage));
                Console.WriteLine("Received RELOAD TABLES command from MS.");
                LoadTablesFromFile(routerTablesFilePath);
            }
            else
            {
                Console.WriteLine("Received unknown command from MS.");
            }
        }

        private Package ReceiveMessage()
        {
            byte[] buffer = new byte[256];
            int bytes = managementSystemSocket.Receive(buffer);
            var message = Encoding.ASCII.GetString(buffer, 0, bytes);
            Package receivedPackage = DeserializeFromJson(message);
            return receivedPackage; 
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

        private string SerializeToJson(Package package)
        {
            string jsonString;
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            jsonString = JsonSerializer.Serialize(package, options);

            return jsonString;
        }

        private Package DeserializeFromJson(string serializedString)
        {
            Package package = new Package();
            package = JsonSerializer.Deserialize<Package>(serializedString);
            return package;
        }

        private void PushLabel(Package package, NhlfeEntry nhlfeEntry)
        {
            if (nhlfeEntry.outLabel == null)
            {
                Console.WriteLine($"Invalid NHLFE entry for router {routerName}. outLabel null.");
                return;
            }
            else if (nhlfeEntry.outPort == null)
            {
                if (nhlfeEntry.nextId == null)
                {
                    Console.WriteLine($"Invalid NHLFE entry for router {routerName}. outPort and nextId null.");
                    return;
                }
                package.labels.Add((int)nhlfeEntry.outLabel);
                var nextNhlfeEntry = nhlfeTable.entries[(int)nhlfeEntry.nextId];
                PushLabel(package, nextNhlfeEntry);
            }
            else if (nhlfeEntry.nextId == null)
            {
                package.incomingPort = (int)nhlfeEntry.outPort;
                // send to cloud
            }
            else
            {
                Console.WriteLine($"Invalid NHLFE entry for router {routerName}. All 3 values not null.");
                return;
            }
        }

        private void PopLabel(Package package, NhlfeEntry nhlfeEntry)
        {
            if (nhlfeEntry.outLabel != null && nhlfeEntry.outPort != null && nhlfeEntry.nextId != null)
            {
                Console.WriteLine($"Invalid NHLFE entry for router {routerName}. outLabel, outPort or nextId  NOT null.");
                return;
            }
            if (package.labels.Any())
            {
                //var poppedLabel = package.labels[package.labels.Count - 1];
                package.labels.RemoveAt(package.labels.Count - 1);
                if (package.labels.Any())
                {
                    var ilmEntry = ilmTable.entries[package.labels.Last()];
                    if (package.incomingPort != ilmEntry.inPort)
                    {
                        Console.WriteLine($"inPort and incomingPort are not equal for router {routerName}.");
                        return;
                    }
                    var newNhlfeEntry = nhlfeTable.entries[ilmEntry.id];
                    switch (newNhlfeEntry.operation)
                    {
                        case "PUSH":
                            PushLabel(package, nhlfeEntry);
                            break;
                        case "POP":
                            PopLabel(package, nhlfeEntry);
                            break;
                        case "SWAP":
                            SwapLabel(package, nhlfeEntry);
                            break;
                    }
                }
                else
                {
                    var mplsEntry = mplsFibTable.entries[package.destPort];
                    if (mplsEntry.fec == 0)
                    {
                        var ipFibEntry = ipFibTable.entries[package.destAddress];
                        package.incomingPort = ipFibEntry.outPort;
                        // send package to cloud
                    }
                }
            }
            else
            {
                Console.WriteLine("No label to pop found.");
            }
        }

        private void SwapLabel(Package package, NhlfeEntry nhlfeEntry)
        {
            package.labels[package.labels.Count - 1] = (int)nhlfeEntry.outLabel;
            package.incomingPort = (int)nhlfeEntry.outPort;
            // send package to cloud
        }

        public void Route(Package package)
        {
            if (routerName == "R2")
            {
                RouteLSR(package);
            }
            else
            {
                RouteLER(package);
            }
        }

        private void RouteLSR(Package package)
        {
            if (!package.labels.Any())
            {
                Console.WriteLine("No label found.");
                return;
            }
            var ilmEntry = ilmTable.entries[package.labels.Last()];
            if (package.incomingPort != ilmEntry.inPort)
            {
                Console.WriteLine($"inPort and incomingPort are not equal for router {routerName}.");
                return;
            }
            if (ilmEntry.poppedLabel != null)
            {
                Console.WriteLine($"poppedLabel is not null for router {routerName}.");
                return;
            }
            var nhlfeEntry = nhlfeTable.entries[ilmEntry.id];
            if (nhlfeEntry.operation != "SWAP")
            {
                Console.WriteLine($"Invalid NHLFE entry for router {routerName}. Different operation than SWAP found.");
                return;
            }
            else if (nhlfeEntry.outLabel == null || nhlfeEntry.outPort == null || nhlfeEntry.nextId != null)
            {
                Console.WriteLine($"Invalid NHLFE entry for router {routerName}.");
                return;
            }
            else
            {
                SwapLabel(package, nhlfeEntry);
            }
        }

        private void RouteLER(Package package)
        {
            if (!package.labels.Any())
            {
                var mplsEntry = mplsFibTable.entries[package.destPort];
                if (mplsEntry.fec == 0)
                {
                    var ipFibEntry = ipFibTable.entries[package.destAddress];
                    package.incomingPort = ipFibEntry.outPort;
                    // send package to cloud
                }
                else
                {
                    var ftnEntry = ftnTable.entries[mplsEntry.fec];
                    var nhlfeEntry = nhlfeTable.entries[ftnEntry.id];
                    if (nhlfeEntry.operation != "PUSH")
                    {
                        Console.WriteLine($"Invalid NHLFE entry for router {routerName}.");
                        return;
                    }
                    PushLabel(package, nhlfeEntry);
                }
            }
            else
            {
                var ilmEntry = ilmTable.entries[package.labels.Last()];
                if (package.incomingPort != ilmEntry.inPort)
                {
                    Console.WriteLine($"inPort and incomingPort are not equal for router {routerName}.");
                    return;
                }
                // tutaj np. jest R3. Tam moze byc lub nie poppedLabel, ale znowu - przez nasze tablice to nic nie wnosi,
                // czy poppedLabel jest.
                /*if (ilmEntry.poppedLabel != null)
                {
                    Console.WriteLine($"poppedLabel is not null for router {routerName}.");
                    return;
                }*/
                var nhlfeEntry = nhlfeTable.entries[ilmEntry.id];
                switch(nhlfeEntry.operation)
                {
                    case "PUSH":
                        PushLabel(package, nhlfeEntry);
                        break;
                    case "POP":
                        PopLabel(package, nhlfeEntry);
                        break;
                    case "SWAP":
                        SwapLabel(package, nhlfeEntry);
                        break;
                }
            }
        }

        private void SendPackageToCloud(Package package)
        {
            try
            {
                cableCloudSocket.Send(Encoding.ASCII.GetBytes(SerializeToJson(package)));
            }
            catch (Exception)
            {
                Console.WriteLine("Couldn't send package to cable cloud.");
            }
        }
    }
}