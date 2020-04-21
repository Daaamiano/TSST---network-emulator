
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace DataStructures
{
    public class Package
    {
        public string sourceName { get; set; }
        public string destAddress { get; set; }
        public int incomingPort { get; set; }
        public int destPort { get; set; }
        public List<int> labels { get; set; }
        public string message { get; set; }

        public Package()
        {
            sourceName = "";
            destAddress = "";
            incomingPort = 0;
            destPort = 0;
            labels = new List<int>();
            message = "";
        }

        public Package(string sourceName, string destAddress, int destPort, string message)
        {
            this.sourceName = sourceName;
            this.destAddress = destAddress;
            incomingPort = 0;
            this.destPort = destPort;
            labels = new List<int>();
            this.message = message;
        }

        public Package(string sourceName, int incomingPort, string destAddress, int destPort, string message)
        {
            this.sourceName = sourceName;
            this.incomingPort = incomingPort;
            this.destAddress = destAddress;
            this.destPort = destPort;
            labels = new List<int>();
            this.message = message;
        }
    }
}
