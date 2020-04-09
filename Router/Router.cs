using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using DataStructures;

namespace Router
{
    class Router
    {

       
        private static readonly byte[] buffer = new byte[2048];
        private const int PORT = 5000;
        private static Socket managementSystemSocket;      // odpowiedzialny za ruch przychodzący


        public string routerName;
        private IPAddress ipAddress;        // adres IP danego routera
        private IPAddress cloudAddress;
        private int cloudPort;
        private IPAddress managementSystemAddress;
        private int managementSystemPort;
        
        private Socket cloudSocket;        // odpowiedzialny za ruch wychodzący
        private static MplsFibTable mplsFibTable;
        private static IpFibTable ipFibTable;
        private static IlmTable ilmTable;
        private static FtnTable ftnTable;
        private static NhlfeTable nhlfeTable;

        public Router(string routerConfigFilePath, string tablesConfigFilePath) 
        {
            LoadPropertiesFromFile(routerConfigFilePath);
            mplsFibTable = new MplsFibTable(tablesConfigFilePath);          //
            ilmTable = new IlmTable(tablesConfigFilePath);                  //ładowanie tablic
            ftnTable = new FtnTable(tablesConfigFilePath);                  //
            ipFibTable = new IpFibTable(tablesConfigFilePath);              //
            nhlfeTable = new NhlfeTable(tablesConfigFilePath);

            Console.Title = "Router";
            ListenForConnections();
            Console.ReadLine();
        }

        private void ListenForConnections()         //Metoda czekająca na połączenie pomiędzy Routerem, a MS.
        {                                          //Gdy wykryje połączenie pomiędzy dwoma węzłami wywoła się funkcja AcceptCallback
            Console.WriteLine("Connecting to Management System...");
            managementSystemSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            managementSystemSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            managementSystemSocket.Listen(0);
            managementSystemSocket.BeginAccept(AcceptCallback, null);
            

        }

        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket;

            try
            {
                socket = managementSystemSocket.EndAccept(AR);
            }
            catch (ObjectDisposedException)
            {
                return;
            }


            socket.BeginReceive(buffer, 0, 2048, SocketFlags.None, ReceiveCallback, socket);
            Console.WriteLine("Client connected, waiting for request...");
            managementSystemSocket.BeginAccept(AcceptCallback, null);
        }
        private static void ReceiveCallback(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;
            int received;

            try
            {
                received = current.EndReceive(AR);
            }
            catch (SocketException)
            {
                Console.WriteLine("Client forcefully disconnected");
                // Don't shutdown because the socket may be disposed and its disconnected anyway.
                current.Close();
                //managementSystemSocket.Remove(current);
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(buffer, recBuf, received);
            string text = Encoding.ASCII.GetString(recBuf);
            Console.WriteLine("Received Text: " + text);
            //Console.WriteLine("Text delivered");

            //if (text.ToLower() == "") // Client requested time
            //{
            //    //Console.WriteLine("Text is a get time request");
            //    byte[] data = Encoding.ASCII.GetBytes("Elo mordo");
            //    current.Send(data);
            //    //Console.WriteLine("Time sent to client");
            //}
            //else if(text.ToLower() == "zdrobniale damian kolakowski")
            //{
            //    Console.WriteLine("H3h3");
            //    byte[] data = Encoding.ASCII.GetBytes("Kolak");
            //    current.Send(data);
            //    //Console.WriteLine("Time sent to client");
            //}
            //else
           // {
                
                
                byte[] data = Encoding.ASCII.GetBytes("Text delivered");
                current.Send(data);
            //}

            current.BeginReceive(buffer, 0, 2048, SocketFlags.None, ReceiveCallback, current);
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

        private string GetProperty(List<string> content, string propertyName)
        {
            return content.Find(line => line.StartsWith(propertyName)).Replace($"{propertyName} ", "");
        }

        private void ConnectToCloud()
        {
            
        }

        private void ConnectToManagementSystem()
        {
            managementSystemSocket = new Socket(managementSystemAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
        }

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