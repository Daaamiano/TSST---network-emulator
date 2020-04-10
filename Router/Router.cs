using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using DataStructures;

namespace Router
{
    class Router
    {
        private static byte[] buffer = new byte[2048];
        private const int PORT = 5000;
        private static Socket managementSystemSocket;

        public string routerName;
        private IPAddress ipAddress;        // adres IP danego routera
        private IPAddress cloudAddress;
        private int cloudPort;
        private IPAddress managementSystemAddress = IPAddress.Parse("127.0.0.1");
        private int managementSystemPort = 5000;
        
        private Socket cloudSocket;  
        private static MplsFibTable mplsFibTable;
        private static IpFibTable ipFibTable;
        private static IlmTable ilmTable;
        private static FtnTable ftnTable;
        private static NhlfeTable nhlfeTable;

        private ManualResetEvent connectDone = new ManualResetEvent(false);

        // do testów
        public Router()
        {
            Console.Title = "Router";
            //Console.ReadLine();
        }

        public Router(string routerConfigFilePath, string tablesConfigFilePath) 
        {
            LoadPropertiesFromFile(routerConfigFilePath);
            mplsFibTable = new MplsFibTable(tablesConfigFilePath);          
            ilmTable = new IlmTable(tablesConfigFilePath);                  
            ftnTable = new FtnTable(tablesConfigFilePath);                  
            ipFibTable = new IpFibTable(tablesConfigFilePath);             
            nhlfeTable = new NhlfeTable(tablesConfigFilePath);

            Console.Title = "Router";
            //Console.ReadLine();
        }

        public void Start()
        {
            managementSystemSocket = new Socket(managementSystemAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            ConnectToManagementSystem();
        }

        private void ConnectToManagementSystem()
        {
            Console.WriteLine("Connecting to management system...");
            /*try
            {
                // Connect to the remote endpoint.  
                managementSystemSocket.BeginConnect(new IPEndPoint(managementSystemAddress, managementSystemPort),
                    new AsyncCallback(ConnectCallback), managementSystemSocket);
                connectDone.WaitOne();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            */
            while (true)
            {
                managementSystemSocket.ReceiveTimeout = 20000;

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
                        Console.ReadLine();

                        break;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Couldn't connect to MS!");
                }
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
            /*
            var file = File.ReadAllLines(configFilePath).ToList();

            routerName = GetProperty(file, "routerName");
            ipAddress = IPAddress.Parse(GetProperty(file, "ipAddress"));
            cloudAddress = IPAddress.Parse(GetProperty(file, "cloudAddress"));
            cloudPort = ushort.Parse(GetProperty(file, "cloudPort"));
            managementSystemAddress = IPAddress.Parse(GetProperty(file, "managementAddress"));
            managementSystemPort = int.Parse(GetProperty(file, "managementPort"));
            */
        }
        /*
        private string GetProperty(List<string> content, string propertyName)
        {
            return content.Find(line => line.StartsWith(propertyName)).Replace($"{propertyName} ", "");
        }
        */

        public void Listen() 
        {
            

        }

        public void Read()
        {

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