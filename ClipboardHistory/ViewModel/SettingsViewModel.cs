using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using ClipboardHistory.Model;

namespace ClipboardHistory.ViewModel
{
    public class SettingsViewModel : BaseViewModel
    {
        private Settings settings;

        public SettingsViewModel()
        {
            settings = new Settings();
            LoadSettings();
        }

        ~SettingsViewModel()
        {
            SaveSettings();
        }

        public void LoadSettings()
        {
            if (File.Exists(Globals.SSettingsFName))
            {
                var streamReader = new StreamReader(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), Globals.SSettingsFName));
                var locker = new object();
                var reader = new XmlTextReader(streamReader);
                try
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(Settings));
                    if (!xmlSerializer.CanDeserialize(reader))
                    {
                        throw new ApplicationException("Ошибка при загрузке настроек");
                    }
                    lock (locker)
                    {
                        settings = (Settings)xmlSerializer.Deserialize(reader);
                    }
                }
                catch (Exception e)
                {
                    throw new ApplicationException(e.Source + '\n' + e.Message);
                }
                finally
                {
                    reader.Close();
                }
            }
        }

        public void SaveSettings()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Settings));
            Stream writer = new FileStream(Globals.SSettingsFName, FileMode.Create);
            try
            {
                xmlSerializer.Serialize(writer, settings);
            }
            finally
            {
                writer.Close();
            }
        }
    }
}
