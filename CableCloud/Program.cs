using System;
using System.IO;

namespace CableCloud
{
    class Program
    {
        static void Main(string[] args) 
        {

            string fileName = "CableCloud.properties";            
            string workingDirectory = Environment.CurrentDirectory;
            string path = Path.Combine(Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName, @"DataStructures", fileName);            
            CableCloud cableCloud = new CableCloud(path);
           // Console.WriteLine("write port number");
            //string port = Console.ReadLine();
            //int result = Int32.Parse(port);
            //cableCloud.Start(result);
        }
    }
}
