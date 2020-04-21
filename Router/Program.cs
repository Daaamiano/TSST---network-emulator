using System;
using System.IO;

namespace Router
{
    class Program
    {
        static void Main(string[] args)
        {
            /*string fileName = "R1.properties";
            string tablesFileName = "R1_tables.properties";
            string workingDirectory = Environment.CurrentDirectory;
            string path = Path.Combine(Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName, @"DataStructures", fileName);
            string tablesPath = Path.Combine(Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName, @"DataStructures", tablesFileName);
            Router router = new Router(path, tablesPath);*/
            Router router = new Router(args[0], args[1]);
            //Console.WriteLine(args[0]);
            //Console.WriteLine(args[1]);
            router.Start(); 
         }

    }
}
