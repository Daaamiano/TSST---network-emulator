using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Host
{
    class Host
    {
        private IpAddress hostSourceAddress;
        private IpAddress hostSourcePort;
        private IpAddress destinationHostPort;
        private IpAddress cableCloudAddress;
        private int cableCloudPort;

        private Socket sendingToCableCloudSocket;
        private Socket receivingFromCableCloudSocket; 

        public Host() 
        {
            Console.Title = "Host";
        }

        public void Start()
        {
            //sendingToCableCloudSocket = new Socket(cableCloudAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            //ConnectToCableCloud();
        }

        private void ConnectToCableCloud()
        {

        }

        public void Listen()
        {
            
        }

        public void Read(IAsyncResult ar)
        {

        }

        public bool Send(string message) 
        {
            try
            {
                Console.WriteLine("Choose which host you want to send the package to: "); // wybieramy numer hosta - np. H1? czy po prostu docelowy numer portu hosta?
                destinationHostPort = int.Parse(Console.ReadLine());
                sendingToCableCloudSocket = new Socket((new IPEndPoint(cableCloudAddress, cableCloudPort)).AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                sendingToCableCloudSocket.Connect(new IPEndPoint(cableCloudAddress, cableCloudPort));
                sendingToCableCloudSocket.Send(new Package(hostSourceAddress, hostSourcePort, destinationHostPort, message).toBytes());
                //messageQueue.Enqueue(Logger.Log("Sent to host with port: " + destinationHostPort + ": " + message, LogType.INFO));
                //sendingToCableCloudSocket.Close();
            }
            catch (Exception e)
            {
                //messageQueue.Enqueue(Logger.Log(e.Message, LogType.ERROR));
               // if (Sender != null) sendingToCableCloudSocket.Close();
                return false;
            }
            return true;
            
        }
    }  
}