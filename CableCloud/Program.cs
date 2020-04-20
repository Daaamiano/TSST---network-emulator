using System;
using System.IO;

namespace CableCloud
{
    class Program
    {
        static void Main(string[] args) 
        {

            //string fileName = "CableCloud.properties";            
            //string workingDirectory = Environment.CurrentDirectory;
            //string path = Path.Combine(Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName, @"DataStructures", fileName);            
            CableCloud cableCloud = new CableCloud(args[0]);
           
        }
    }
}
