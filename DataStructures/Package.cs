using System.Collections.Generic;

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
            sourceName = default;
            destAddress = default;
            incomingPort = default;
            destPort = default;
            labels = default;
            message = default;
        }

        public Package(string sourceName, string destAddress, int destPort, string message)
        {
            this.sourceName = sourceName;
            this.destAddress = destAddress;
            this.destPort = destPort;
            this.message = message;
        }

        public Package(int incomingPort, string destAddress, int destPort, string message) 
        {
            this.incomingPort = incomingPort;
            this.destAddress = destAddress;
            this.destPort = destPort;
            this.message = message;
        }

        public Package(string sourceName, string destAddress, int destPort, List<int> labels, string message)
        {
            this.sourceName = sourceName;
            this.destAddress = destAddress;
            this.destPort = destPort;
            this.labels = labels;
            this.message = message;
        }
    }
}
