using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using DataStructures;

namespace Router
{
    class Router
    {
        public string routerName;
        private IPAddress ipAddress;        // adres IP danego routera
        private IPAddress cloudAddress;
        private int cloudPort;
        private IPAddress managementSystemAddress;
        private int managementSystemPort;
        private Socket managementSystemSocket;      // odpowiedzialny za ruch przychodzący
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