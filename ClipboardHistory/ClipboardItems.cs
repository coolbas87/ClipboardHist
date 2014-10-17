using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClipboardHistory.Model;

namespace ClipboardHistory
{
    public class ClipboardItems : ObservableCollection<Clip>
    {
        public ClipboardItems()
            : base()
        { }
        public ClipboardItems(IEnumerable<Clip> collection)
            : base(collection)
        { }
        public ClipboardItems(List<Clip> list)
            : base(list)
        { }

        public void WriteToFile()
        {
            if (Count > 0)
            {
                string fName = Globals.HistoryFile;
                Stream dest = null;
                try
                {
                    dest = File.Open(fName, FileMode.Create, FileAccess.ReadWrite);
                    BinaryWriter writer = new BinaryWriter(dest);
                    writer.Write(this.Count);
                    foreach (Clip item in this)
                    {
                        item.WriteToStream(dest);
                    }
                }
                finally
                {
                    if (dest != null)
                    {
                        dest.Close();
                    }
                }
            }
        }

        public void ReadFromFile()
        {
            FileInfo fi = null;
            Stream source = null;
            string histFile = Globals.HistoryFile;
            try
            {
                fi = new FileInfo(histFile);
                if (fi.Exists)
                {
                    source = File.Open(histFile, FileMode.Open, FileAccess.Read);
                    BinaryReader reader = new BinaryReader(source);
                    int itemsCount = reader.ReadInt32();
                    for (int i = 0; i < itemsCount; ++i)
                    {
                        Clip clip = new Clip(reader);
                        Insert(i, clip);
                    }
                }
            }
            catch
            {
                Clear();

            }
            finally
            {
                if (source != null)
                {
                    source.Close();
                }
            }
        }
    }
}
