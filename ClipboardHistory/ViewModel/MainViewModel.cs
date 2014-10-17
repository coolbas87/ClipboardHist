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

namespace ClipboardHistory.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private Command addCommand;
        private Command clipboardChangedCommand;
        private Command setClipboardItemCommand;

        private ClipboardItems clipboardItems;
        private object lastClipbrdObject;
        private GlobalHotkey showHotKey = null;
 
        public MainViewModel()
        {
            clipboardItems = new ClipboardItems();
            clipboardItems.ReadFromFile();
            if (clipboardItems.Count > 0)
            {
                lastClipbrdObject = clipboardItems[0].ItemObject;
            }
            else
            {
                ClipboardChanged();
            }
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

        public ICommand AddCommand
        {
            get
            {
                if (addCommand == null)
                {
                    addCommand = new Command(param => AddItem());
                }

                return addCommand;
            }
        }

        public ICommand cmdClipboardChanged
        {
            get
            {
                if (clipboardChangedCommand == null)
                {
                    clipboardChangedCommand = new Command(param => ClipboardChanged());
                }

                return clipboardChangedCommand;
            }
        }

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

        private void ClipboardChanged()
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

                if ((lastClipbrdObject is WriteableBitmap) && !IsImagesEqual((BitmapSource)lastClipbrdObject, bs))
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

                if (((lastClipbrdObject is StringCollection) && !IsStringCollectionsEqual((StringCollection)lastClipbrdObject, fileList)) ||
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

        private void ClearHistory()
        {
            //if ((clipboardItems != null) && (clipboardItems.Count > Globals.Settings.ListCount))
            //{
            //    for (int i = clipboardItems.Count - 1; i >= Globals.Settings.ListCount; i--)
            //    {
            //        clipboardItems.RemoveAt(i);
            //    }
            //}
        }

        private bool IsStringCollectionsEqual(StringCollection col1, StringCollection col2)
        {
            List<string> list1 = col1.Cast<string>().ToList();
            List<string> list2 = col2.Cast<string>().ToList();
            List<string> resList = list1.Except(list2).ToList();
            return resList.Count == 0;
        }

        private bool IsImagesEqual(BitmapSource img1, BitmapSource img2)
        {
            MemoryStream ms = new MemoryStream();
            BitmapEncoder encoder = new BmpBitmapEncoder();

            encoder.Frames.Add(BitmapFrame.Create(img1));
            encoder.Save(ms);
            String image1 = Convert.ToBase64String(ms.ToArray());
            ms.Position = 0;

            encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(img2));
            encoder.Save(ms);
            String image2 = Convert.ToBase64String(ms.ToArray());
            ms.Close();
            return image1.Equals(image2);
        }

        public void AddItem()
        {
            Random rnd = new Random();
            string itemText = rnd.Next(999).ToString();
            Clip clip = new Clip(itemText, itemText, "Text");
            clipboardItems.Add(clip);
        }

        public void SetClipboardItem(int index)
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
                //if (this.WindowState != System.Windows.WindowState.Minimized)
                //{
                //    this.Close();
                //}
            }
        }
    }
}
