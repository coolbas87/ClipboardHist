using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace ClipboardHistory.Model
{
    [XmlRoot(Globals.SSettings)]
    public class Settings
    {
        [XmlElement(Globals.SListCount)]
        public int ListCount { get; set; }

        [XmlElement(Globals.STrayMenuCount)]
        public int TrayMenuCount { get; set; }

        public string HistoryFile
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), Globals.HistoryFile);
            }
        }

        [XmlElement(Globals.SWindowState)]
        public WindowState WindowState { get; set; }

        [XmlElement(Globals.SHeight)]
        public Double Height { get; set; }

        [XmlElement(Globals.SWidth)]
        public Double Width { get; set; }

        [XmlElement(Globals.STop)]
        public Double Top { get; set; }

        [XmlElement(Globals.SLeft)]
        public Double Left { get; set; }

        public Settings()
        {
            ListCount = Globals.DefListCountValue;
            TrayMenuCount = Globals.DefTrayMenuCountValue;
            WindowState = System.Windows.WindowState.Normal;
        }
    }
}
