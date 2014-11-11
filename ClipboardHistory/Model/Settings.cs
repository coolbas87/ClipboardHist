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
    public class Settings
    {
        private static Settings current;
        private static readonly object syncObj = new object();

        public Settings()
        {
            ListCount = 50;
            Height = 500;
            Width = 340;
            Top = 0;
            Left = 0;
            WindowState = WindowState.Normal;
        }

        public static Settings Current
        {
            get
            {
                if (current != null)
                    return current;

                lock (syncObj)
                {
                    if (current == null)
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                        string fileName = AppDomain.CurrentDomain.BaseDirectory + Globals.SSettingsFName;
                        if (File.Exists(fileName))
                        {
                            using (FileStream stream = File.Open(fileName, FileMode.Open))
                                current = (Settings)serializer.Deserialize(stream);
                        } 
                        else
                        {
                            current = new Settings();
                        }
                    }
                }

                return current;
            }
        }

        public static void SaveSettings()
        {
            if (current == null)
                return;

            lock (syncObj)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                using (FileStream stream = File.Create(AppDomain.CurrentDomain.BaseDirectory + Globals.SSettingsFName))
                    serializer.Serialize(stream, current);
            }
        }

        #region properties
        public int ListCount { get; set; }
        //public int TrayMenuCount { get; set; }

        public string HistoryFile
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), Globals.HistoryFile);
            }
        }

        public WindowState WindowState { get; set; }
        public Double Height { get; set; }
        public Double Width { get; set; }
        public Double Top { get; set; }
        public Double Left { get; set; }
        #endregion
    }
}
