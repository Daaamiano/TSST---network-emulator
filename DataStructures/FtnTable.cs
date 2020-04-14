using System.Collections.Generic;
using System.IO;

namespace DataStructures
{
    public class FtnTable
    {
        // Naszym kluczem dla ka�dej pary w s�owniku jest to, po czym przeszukujemy tablic�, a warto�ci� reszta p�l (obudowane klas� reprezentuj�c� wpis dla odpowiedniej tablicy).
        // Zatem w przypadku tablicy FTN kluczem b�dzie FEC, a warto�ci� ID (jedyne pole w klasie FtnEntry).
        Dictionary<int, FtnEntry> entries = new Dictionary<int, FtnEntry>();

        public FtnTable(string configFilePath, string routerName)
        {
            string rowName = routerName + "_FTN";
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
                var entry = new FtnEntry(int.Parse(splitRow[2]));
                entries.Add(int.Parse(splitRow[1]), entry);
                
            }
        }
    }
}
