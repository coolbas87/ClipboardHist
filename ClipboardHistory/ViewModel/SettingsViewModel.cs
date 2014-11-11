using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using ClipboardHistory.Model;
using ClipboardHistory.View;
using System.Windows.Input;

namespace ClipboardHistory.ViewModel
{
    public class SettingsViewModel : BaseViewModel
    {
        private SettingsView window;
        private Command okCommand;

        public int ListCount { get; set; }

        public SettingsViewModel(SettingsView view)
        {
            window = view;
            ListCount = Settings.Current.ListCount;
        }

        public ICommand OkCommand
        {
            get
            {
                if (okCommand == null)
                {
                    okCommand = new Command(param => SetSettings());
                }

                return okCommand;
            }
        }

        private void SetSettings()
        {
            Settings.Current.ListCount = ListCount;
            window.Close();
        }
    }
}
