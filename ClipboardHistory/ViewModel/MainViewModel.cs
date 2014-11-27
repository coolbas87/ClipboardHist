using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Linq;
using ClipboardHistory.Model;
using System.Windows.Controls;
using ClipboardHistory.View;
using System.Windows.Interop;
using System.Windows.Media;

namespace ClipboardHistory.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        #region Commands declaration
        private Command setClipboardItemCommand;
        private Command clearHistoryCommand;
        private Command exitCommand;
        private Command openSettingsCommand;
        #endregion

        private MainWindow window;
        private ClipboardWatcher clipboardWatcher;
        private HwndSource hWndSource;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private static System.Windows.Forms.ContextMenu contextMenu;

        private ClipboardItems clipboardItems;
        private object lastClipbrdObject;
        private GlobalHotkey showHotKey = null;
        private bool terminating = false;

        public MainViewModel(MainWindow mainWindow)
        {
            window = mainWindow;
            window.Closed += window_Closed;
            window.Closing += window_Closing;
            window.Loaded += window_Loaded;
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Icon = Properties.Resources.mainicon;
            notifyIcon.MouseClick += notifyIcon_MouseClick;
            notifyIcon.ContextMenu = GetContextMenu();
            notifyIcon.Visible = true;
            RestoreFormParams();
            clipboardItems = new ClipboardItems();
            clipboardItems.ReadFromFile();
            if (clipboardItems.Count > 0)
            {
                lastClipbrdObject = clipboardItems[0].ItemObject;
            }
        }

        #region Methods
        private System.Windows.Forms.ContextMenu GetContextMenu()
        {
            if (contextMenu == null)
            {
                contextMenu = new System.Windows.Forms.ContextMenu();
                System.Windows.Forms.MenuItem item = null;
                item = new System.Windows.Forms.MenuItem();
                item.Text = Globals.SRestore;
                item.DefaultItem = true;
                item.Click += delegate(object sender, EventArgs args)
                {
                    RestoreWindow();
                };
                contextMenu.MenuItems.Add(item);
                item = new System.Windows.Forms.MenuItem();
                item.Text = "-";
                contextMenu.MenuItems.Add(item);
                item = new System.Windows.Forms.MenuItem();
                item.Text = Globals.SExit;
                item.Click += delegate(object sender, EventArgs args)
                {
                    Exit();
                };
                contextMenu.MenuItems.Add(item);
            }
            return contextMenu;
        }

        private void RestoreWindow()
        {
            window.Show();
            if (!window.IsFocused)
            {
                window.Activate();
            }
            Settings.Current.WasClosed = false;
        }

        private void RestoreFormParams()
        {
            window.WindowState = Settings.Current.WindowState;
            window.Height = Settings.Current.Height;
            window.Width = Settings.Current.Width;
            window.Top = Settings.Current.Top;
            window.Left = Settings.Current.Left;
        }

        public ClipboardItems ClipboardItems
        {
            get
            {
                return clipboardItems;
            }
            set
            {
                clipboardItems = value;
            }
        }

        private void ClearHistory()
        {
            if ((clipboardItems != null) && (clipboardItems.Count > Settings.Current.ListCount))
            {
                for (int i = clipboardItems.Count - 1; i >= Settings.Current.ListCount; i--)
                {
                    clipboardItems.RemoveAt(i);
                }
            }
        }

        private void StoreFormParams()
        {
            Settings.Current.Height = window.Height;
            Settings.Current.Width = window.Width;
            Settings.Current.Top = window.Top;
            Settings.Current.Left = window.Left;                  
            Settings.Current.WindowState = window.WindowState;
        }

        private void ClipboardChanged(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                string textDataFormat = DataFormats.Text;

                if (Clipboard.ContainsData(DataFormats.Html))
                {
                    textDataFormat = DataFormats.Html;
                }
                else if (Clipboard.ContainsData(DataFormats.Rtf))
                {
                    textDataFormat = DataFormats.Rtf;
                }
                else if (Clipboard.ContainsData(DataFormats.UnicodeText))
                {
                    textDataFormat = DataFormats.UnicodeText;
                }

                object textFromClipboard = Clipboard.GetData(textDataFormat);
                if (!object.Equals(lastClipbrdObject, textFromClipboard))
                {
                    string name = Clipboard.GetText();
                    lastClipbrdObject = textFromClipboard;
                    clipboardItems.Insert(0, new Clip(name, lastClipbrdObject, textDataFormat));
                }
            }
            else if (Clipboard.ContainsImage())
            {
                BitmapSource bs = Clipboard.GetImage();

                if (((lastClipbrdObject is WriteableBitmap) && !Utils.IsImagesEqual((BitmapSource)lastClipbrdObject, bs)) || !(lastClipbrdObject is ImageSource))
                {
                    WriteableBitmap imgFromClipboard = new WriteableBitmap(bs);
                    string imgName = string.Format(Globals.SIMAGE, imgFromClipboard.Height, imgFromClipboard.Width);
                    lastClipbrdObject = imgFromClipboard;
                    clipboardItems.Insert(0, new Clip(imgName, lastClipbrdObject, DataFormats.Bitmap));
                }
            }
            else if (Clipboard.ContainsFileDropList())
            {
                StringCollection fileList = Clipboard.GetFileDropList();

                if (((lastClipbrdObject is StringCollection) && !Utils.IsStringCollectionsEqual((StringCollection)lastClipbrdObject, fileList)) ||
                    !(lastClipbrdObject is StringCollection))
                {
                    string name = "";
                    foreach (string item in fileList)
                    {
                        name += item + "; ";
                    }
                    lastClipbrdObject = fileList;
                    clipboardItems.Insert(0, new Clip(string.Format(Globals.SFILE_LIST, name), fileList,
                        DataFormats.FileDrop));
                }
            }
            ClearHistory();
            clipboardItems.WriteToFile();
        }
        #endregion

        #region Events
        private void DoOnShowHotKey(object sender, HotkeyEventArgs e)
        {
            RestoreWindow();
        }

        private void notifyIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                RestoreWindow();
            }
        }

        void window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper wih = new WindowInteropHelper(window);
            hWndSource = HwndSource.FromHwnd(wih.Handle);
            clipboardWatcher = new ClipboardWatcher(hWndSource.Handle);
            clipboardWatcher.OnClipboardChanged += new EventHandler(ClipboardChanged);
            clipboardWatcher.SetClipboardHandle();
            hWndSource.AddHook(clipboardWatcher.ReceiveMessage);
            showHotKey = new GlobalHotkey(Modifiers.Win, Keys.Oemtilde, window, true);
            showHotKey.HotkeyPressed += new EventHandler<HotkeyEventArgs>(DoOnShowHotKey);
            if (Settings.Current.WasClosed)
            {
                window.Hide();
            }
        }

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if ((sender is Window) && !(terminating))
            {
                ((Window)sender).Hide();
                e.Cancel = true;
                Settings.Current.WasClosed = true;
            }
        }

        private void window_Closed(object sender, EventArgs e)
        {
            clipboardItems.WriteToFile();
            StoreFormParams();
            Settings.SaveSettings();
            showHotKey.Dispose();
            showHotKey = null;
            notifyIcon.Dispose();
            notifyIcon = null;
        }
        #endregion

        #region Impl Commands
        public ICommand cmdSetClipboardItem
        {
            get
            {
                if (setClipboardItemCommand == null)
                {
                    setClipboardItemCommand = new Command(param => SetClipboardItem((int)param));
                }
                return setClipboardItemCommand;
            }
        }

        public ICommand cmdClearHistory
        {
            get
            {
                if (clearHistoryCommand == null)
                {
                    clearHistoryCommand = new Command(param => ClearAllHistory());
                }
                return clearHistoryCommand;
            }
        }

        public ICommand cmdExit
        {
            get
            {
                if (exitCommand == null)
                {
                    exitCommand = new Command(param => Exit());
                }
                return exitCommand;
            }
        }

        public ICommand cmdOpenSettings
        {
            get
            {
                if (openSettingsCommand == null)
                {
                    openSettingsCommand = new Command(param => OpenSettings());
                }
                return openSettingsCommand;
            }
        }

        private void SetClipboardItem(int index)
        {
            if (index > -1)
            {
                Clip clip = clipboardItems[index];
                Clipboard.Clear();
                DataObject dataObj = new DataObject();
                if (clip.ObjectFormat.Equals(DataFormats.Html))
                {
                    dataObj.SetData(DataFormats.Html, clip.ItemObject);
                    dataObj.SetData(DataFormats.UnicodeText, clip.Text);
                    dataObj.SetData(DataFormats.Text, clip.Text);
                }
                else if (clip.ObjectFormat.Equals(DataFormats.Rtf))
                {
                    dataObj.SetData(DataFormats.Rtf, clip.ItemObject);
                    dataObj.SetData(DataFormats.UnicodeText, clip.Text);
                    dataObj.SetData(DataFormats.Text, clip.Text);
                }
                else if (clip.ObjectFormat.Equals(DataFormats.UnicodeText))
                {
                    dataObj.SetData(DataFormats.Text, clip.ItemObject);
                    dataObj.SetData(DataFormats.UnicodeText, clip.ItemObject);
                }
                else if (clip.ObjectFormat.Equals(DataFormats.Text))
                {
                    dataObj.SetData(DataFormats.Text, clip.ItemObject);
                }
                else if (clip.ObjectFormat.Equals(DataFormats.Bitmap))
                {
                    dataObj.SetImage((WriteableBitmap)clip.ItemObject);
                }
                else if (clip.ObjectFormat.Equals(DataFormats.FileDrop))
                {
                    dataObj.SetFileDropList((StringCollection)clip.ItemObject);
                }
                Clipboard.SetDataObject(dataObj);
                window.Close();
            }
        }

        private void ClearAllHistory()
        {
            clipboardItems.Clear();
        }

        private void OpenSettings()
        {
            SettingsView settings = new SettingsView();
            settings.DataContext = new SettingsViewModel(settings);
            settings.ShowDialog();
        }

        private void Exit()
        {
            Application.Current.Shutdown();
            terminating = true;
        }
        #endregion
    }
}
