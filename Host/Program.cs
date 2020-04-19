using System;
using System.IO;


namespace Host
{
    class Program
    {
        static void Main(string[] args)
        {
			//string fileName = "host2.properties";
			string workingDirectory = Environment.CurrentDirectory;
			string path = Path.Combine(Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName, @"Host", args[0]);
            Host host = new Host(path);  
            //Host host = new Host(@"C:\Users\Daniel\Source\Repos\TSST_20L\Host\host1.properties");
        }

    }
}
