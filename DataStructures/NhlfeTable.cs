using System;
using System.Collections.Generic;
using System.IO;

namespace DataStructures
{
    public class NhlfeTable
    {
        // Naszym kluczem dla każdej pary w słowniku jest to, po czym przeszukujemy tablicę, a wartością reszta pól (obudowane klasą reprezentującą wpis dla odpowiedniej tablicy).
        Dictionary<int, NhlfeEntry> entries = new Dictionary<int, NhlfeEntry>();

        public NhlfeTable(string configFilePath, string routerName)
        {
            string rowName = routerName + "_NHLFE";
            LoadTableFromFile(configFilePath, rowName);
            //test
            /*
            Console.WriteLine("NHLFE {0}:", routerName);
            foreach (var entry in entries)
            {
                Console.WriteLine(entry.Value.operation);
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
                if (splitRow[3] == "-" && splitRow[3] == "-" && splitRow[3] == "-")
                {
                    var entry = new NhlfeEntry(splitRow[2], null, null, null);
                    entries.Add(int.Parse(splitRow[1]), entry);
                }
                else if (splitRow[3] == "-")
                {
                    var entry = new NhlfeEntry(splitRow[2], null, int.Parse(splitRow[4]), int.Parse(splitRow[5]));
                    entries.Add(int.Parse(splitRow[1]), entry);
                } 
                else if (splitRow[4] == "-")
                {
                    var entry = new NhlfeEntry(splitRow[2], int.Parse(splitRow[3]), null, int.Parse(splitRow[5]));
                    entries.Add(int.Parse(splitRow[1]), entry);
                }
                else if (splitRow[5] == "-")
                {
                    var entry = new NhlfeEntry(splitRow[2], int.Parse(splitRow[3]), int.Parse(splitRow[4]), null);
                    entries.Add(int.Parse(splitRow[1]), entry);
                }
                else
                {
                    Console.WriteLine("Unknown NHLFE entry.");
                }
            }
        }
    }
}
