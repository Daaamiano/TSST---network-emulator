using System;

namespace CableCloud
{
    class Program
    {
        static void Main(string[] args) 
        {
            Console.WriteLine("Hello my friend am your cable cloud");
            CableCloud cableCloud = new CableCloud();
            cableCloud.Start();
        }
    }
}
