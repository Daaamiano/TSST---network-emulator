using System;
using System.Collections.Generic;
using System.IO;

namespace DataStructures
{
    public class MplsFibTable
    {
        // Naszym kluczem dla każdej pary w słowniku jest to, po czym przeszukujemy tablicę,
        // a wartością reszta pól (obudowane klasą reprezentującą wpis dla odpowiedniej tablicy).
        // Kluczem jest destPort.
        public Dictionary<int, MplsFibEntry> entries = new Dictionary<int, MplsFibEntry>();

        public MplsFibTable(string configFilePath, string routerName)
        {
            string rowName = routerName + "_MPLSFIB";
            LoadTableFromFile(configFilePath, rowName);
            //test
            /*
            Console.WriteLine("MPLS {0}:", routerName);
            foreach (var entry in entries)
            {
                Console.WriteLine(entry.Value.fec);
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
                var entry = new MplsFibEntry(int.Parse(splitRow[2]));
                entries.Add(int.Parse(splitRow[1]), entry);
            }
        }

        public void AddRowToTable(string tablePath, string routerName, int destPortAdd, int fecAdd)
        {
            entries.Add(destPortAdd, new MplsFibEntry(fecAdd));
            using (StreamWriter file = new StreamWriter(tablePath, true))
            {
                file.WriteLine(routerName + "_MPLSFIB, {0}, {1}", destPortAdd, fecAdd);
            }

            Console.WriteLine($"\nSaved {routerName} MPLS-FIB table to file.\n");
        }
 
        public void DeleteRowFromTable(string row, string tablePath)
        {
            int counter = 1;
            entries.Remove(int.Parse(row));
            try
            {
                foreach (var entry in File.ReadAllLines(tablePath))
                {
                    var splitRow = entry.Split(", ");

                    if (splitRow.Length == 1)
                    {
                        counter++;
                        continue;
                    }

                    if (splitRow[1] == row)
                    {
                        break;
                    }
                    else
                    {
                        counter++;
                    }
                }

                string[] lines = File.ReadAllLines(tablePath);
                using (StreamWriter writer = new StreamWriter(tablePath))
                {
                    for (int currentLine = 1; currentLine <= lines.Length; ++currentLine)
                    {
                        if (currentLine == counter)
                        {
                            continue;

                        }
                        else
                        {
                            writer.WriteLine(lines[currentLine - 1]);
                        }
                    }
                    Console.WriteLine("Deleted entry.");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void PrintEntries()
        {
            int i = 1;
            Console.WriteLine("Index, DestinationPort, FEC");
            foreach (KeyValuePair<int, MplsFibEntry> kvp in entries)
            {
                Console.WriteLine(i + ". {0}, {1}", kvp.Key, kvp.Value.fec);
                i++;
            }
        }
    }
}
