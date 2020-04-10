using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CableCloud
{
    class CableCloud
    {
        private IPAddress cloudAddress;
        private int cloudPort;
        private Socket managementSystemSocket;      // odpowiedzialny za ruch przychodzący
        private Socket cloudSocket;        // odpowiedzialny za ruch wychodzący
        //ustalic czy 2 porty czy jeden, plus robic na local host , port to 5000

        public CableCloud()
        {
            //przypomniec sobie czy potrzebuje wczytac tablice
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
    }
}
