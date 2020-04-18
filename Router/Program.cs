using System;
using System.IO;

namespace Router
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileName = "R1.properties";
            string tablesFileName = "R1_tables.properties";
            string workingDirectory = Environment.CurrentDirectory;
            string path = Path.Combine(Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName, @"DataStructures", fileName);
            string tablesPath = Path.Combine(Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName, @"DataStructures", tablesFileName);
            Router router = new Router(path, tablesPath);
            router.Start(); 
         }

    }
}
