using System.Collections.Generic;
using System.IO;

namespace DataStructures
{
    public class MplsFibTable
    {
        // Naszym kluczem dla każdej pary w słowniku jest to, po czym przeszukujemy tablicę, a wartością reszta pól (obudowane klasą reprezentującą wpis dla odpowiedniej tablicy).
        Dictionary<int, MplsFibEntry> entries = new Dictionary<int, MplsFibEntry>();

        public MplsFibTable(string configFilePath, string routerName)
        {
            string rowName = routerName + "_MPLSFIB";
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
                var entry = new MplsFibEntry(int.Parse(splitRow[2]));
                entries.Add(int.Parse(splitRow[1]), entry);
            }
        }
    }
}
