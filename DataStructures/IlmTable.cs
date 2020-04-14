using System;
using System.Collections.Generic;
using System.IO;

namespace DataStructures
{
    public class IlmTable
    {
        // Naszym kluczem dla każdej pary w słowniku jest to, po czym przeszukujemy tablicę, a wartością reszta pól (obudowane klasą reprezentującą wpis dla odpowiedniej tablicy).
        Dictionary<int, IlmEntry> entries = new Dictionary<int, IlmEntry>();

        public IlmTable(string configFilePath, string routerName)
        {
            string rowName = routerName + "_ILM";
            LoadTableFromFile(configFilePath, rowName);
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
                if (splitRow[3] == "-")
                {
                    var entry = new IlmEntry(int.Parse(splitRow[2]), null, int.Parse(splitRow[4]));
                    entries.Add(int.Parse(splitRow[1]), entry);
                }
                else
                {
                    var entry = new IlmEntry(int.Parse(splitRow[2]), int.Parse(splitRow[3]), int.Parse(splitRow[4]));
                    entries.Add(int.Parse(splitRow[1]), entry);
                }
            }
        }
    }
}
