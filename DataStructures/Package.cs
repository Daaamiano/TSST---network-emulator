using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace DataStructures
{
    public class Package
    {
        public string destAddress { get; set; }
        public int destPort { get; set; }
        public List<int> labels { get; set; }
        public string message { get; set; }

        public Package()
        {
            destAddress = default;
            destPort = default;
            labels = default;
            message = default;
        }

        public Package(string destAddress, int destPort)
        {
            this.destAddress = destAddress;
            this.destPort = destPort;
        }

        public Package(string destAddress, int destPort, List<int> labels, string message)
        {
            this.destAddress = destAddress;
            this.destPort = destPort;
            this.labels = labels;
            this.message = message;
        }
    }
}
