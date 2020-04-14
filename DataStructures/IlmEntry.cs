using System;

namespace DataStructures
{
    public struct IlmEntry
    {

        public int inPort;
        public int? poppedLabel;
        public int id;


        public IlmEntry(int inPort, int? poppedLabel, int id)
        {
            this.inPort = inPort;
            this.poppedLabel = poppedLabel;
            this.id = id;
        }
    }
}