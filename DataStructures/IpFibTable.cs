using System;
using System.Collections.Generic;
using System.IO;

namespace DataStructures
{
    public class IpFibTable
    {
        // Naszym kluczem dla każdej pary w słowniku jest to, po czym przeszukujemy tablicę, a wartością reszta pól (obudowane klasą reprezentującą wpis dla odpowiedniej tablicy).
        Dictionary<string, IpFibEntry> entries = new Dictionary<string, IpFibEntry>();

        public IpFibTable(string configFilePath, string routerName)
        {
            string rowName = routerName + "_IPFIB";
            LoadTableFromFile(configFilePath, rowName);
            //test
            /*
            Console.WriteLine("IPFIB {0}:", routerName);
            foreach (var entry in entries)
            {
                Console.WriteLine(entry.Value.outPort);
            }
            */
        }

        private void LoadTableFromFile(string configFilePath, string rowName)
        {
            foreach (var row in File.ReadAllLines(configFilePath))
            {
                var splitRow = row.Split(", ");
                if (splitRow[0] != rowName)
                {
                    continue;
                }
                var entry = new IpFibEntry(int.Parse(splitRow[2]));
                entries.Add(splitRow[1], entry);
            }
        }
    }
}
