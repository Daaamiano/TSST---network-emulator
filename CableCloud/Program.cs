using System;

namespace CableCloud
{
    class Program
    {
        static void Main(string[] args) 
        {
            CableCloud cableCloud = new CableCloud();
            Console.WriteLine("write port number");
            string port = Console.ReadLine();
            int result = Int32.Parse(port);
            cableCloud.Start(result);
        }
    }
}
