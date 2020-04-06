using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

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
        private Socket receiverSocket;      // odpowiedzialny za ruch przychodzący
        private Socket senderSocket;        // odpowiedzialny za ruch wychodzący

        public Router(string configFilePath) 
        {
            loadPropertiesFromFile(configFilePath);

        }

        private void loadPropertiesFromFile(string configFilePath)
        {
            var file = File.ReadAllLines(configFilePath).ToList();

            routerName = GetProperty(file, "routerName");
            ipAddress = IPAddress.Parse(GetProperty(file, "ipAddress"));
            cloudAddress = IPAddress.Parse(GetProperty(file, "cloudAddress"));
            cloudPort = ushort.Parse(GetProperty(file, "cloudPort"));
            managementSystemAddress = IPAddress.Parse(GetProperty(file, "managementAddress"));
            managementSystemPort = int.Parse(GetProperty(file, "managementPort"));
        }

        private string GetProperty(List<string> content, string propertyName)
        {
            return content.Find(line => line.StartsWith(propertyName)).Replace($"{propertyName} ", "");
        }

        public void Listen() { }

        public void Read() { }

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