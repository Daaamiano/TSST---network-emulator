using System;
using System.Collections.Generic;
using System.IO;

namespace DataStructures
{
    public class IlmTable
    {
        // Naszym kluczem dla każdej pary w słowniku jest to, po czym przeszukujemy tablicę,
        // a wartością reszta pól (obudowane klasą reprezentującą wpis dla odpowiedniej tablicy).
        // Kluczem jest inLabel.
        public Dictionary<int, IlmEntry> entries = new Dictionary<int, IlmEntry>();

        public IlmTable(string configFilePath, string routerName)
        {
            string rowName = routerName + "_ILM";
            LoadTableFromFile(configFilePath, rowName);
            // test:
            /*
            Console.WriteLine("ILM {0}: ", routerName);
            foreach (var entry in entries)
            {
                Console.WriteLine(entry.Value.inPort);
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
                else if (splitRow[3] == "-")
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

        public void AddRowToTable(string tablePath, string routerName, int inLabelAdd, int inPortAdd, int? poppedLabelAdd, int idAdd)
        {
            entries.Add(inLabelAdd, new IlmEntry(inPortAdd, poppedLabelAdd, idAdd));
            using (StreamWriter file = new StreamWriter(tablePath, true))
            {
                file.WriteLine(routerName + "_ILM, {0}, {1}, {2}, {3}", inLabelAdd, inPortAdd, poppedLabelAdd, idAdd);
            }

            Console.WriteLine($"\nSaved {routerName} ILM table to file");
        }

        public void DeleteRowFromTable(string row, string tablePath)
        {
            int counter = 1;
            entries.Remove(int.Parse(row));
            try
            {
                string[] lines = File.ReadAllLines(tablePath);
                foreach (var entry in lines)
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
                    Console.WriteLine("Deleted entry from.");
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
            Console.WriteLine("Index, InLabel, InPort, PoppedLabel, ID");
            foreach (KeyValuePair<int, IlmEntry> kvp in entries)
            {
                Console.WriteLine(i + ". {0}, {1}, {2}, {3}", kvp.Key, kvp.Value.inPort, kvp.Value.poppedLabel, kvp.Value.id);
                i++;
            }
        }
    }
}
