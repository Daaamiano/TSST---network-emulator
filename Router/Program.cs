using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

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
            string pathTables = Path.Combine(Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName, @"DataStructures", tablesFileName);
            Router router = new Router(path, pathTables);
            router.Start(); 
         }

    }
}
