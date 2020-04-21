using System;
using System.IO;


namespace Host
{
    class Program
    {
        static void Main(string[] args)
        {
            //Host host = new Host(args[0]);
            string fileName = "host1.properties";
            string workingDirectory = Environment.CurrentDirectory;
            string path = Path.Combine(Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName, @"DataStructures", fileName);
            Host host = new Host(path);
        }

    }
}
