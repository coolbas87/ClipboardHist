using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ClipboardHistory.Model
{
    public class Clip
    {
        private object itemObject;
        private string objectFormat;
        private string text;

        public string DisplayText
        {
            get { return Text.Replace("\r", " ").Replace("\n", " "); }
        }

        public object ItemObject
        { 
            get
            {
                return itemObject;
            }
            set
            {
                itemObject = value;
            } 
        }

        public string ObjectFormat
        { 
            get
            {
                return objectFormat;
            }
            set
            {
                objectFormat = value;
            }
        }

        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
            }
        }

        public Clip(BinaryReader reader)
        {
            text = string.Empty;
            objectFormat = string.Empty;
            itemObject = null;
            ReadFromStream(reader);
        }

        public Clip(string clipText, object clipObject, string clipFormat)
        {
            text = clipText;
            itemObject = clipObject;
            objectFormat = clipFormat;
        }

        private static byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }

        private static Object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            Object obj = (Object)binForm.Deserialize(memStream);
            return obj;
        }

        private static byte[] ImageToByteArray(BitmapSource image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(ms);
                return ms.ToArray();
            }
        }

        private static WriteableBitmap ByteArrayToImage(byte[] arrBytes)
        {
            using (MemoryStream ms = new MemoryStream(arrBytes, 0, arrBytes.Length))
            {
                ms.Write(arrBytes, 0, arrBytes.Length);
                BitmapImage bs = new BitmapImage();
                bs.BeginInit();
                try
                {
                    bs.StreamSource = ms;
                    bs.CacheOption = BitmapCacheOption.OnLoad;
                }
                finally
                {
                    bs.EndInit();
                }
                bs.Freeze();
                WriteableBitmap bitmap = new WriteableBitmap(bs);
                return bitmap;
            }
        }

        public void WriteToStream(Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(text);
            writer.Write(objectFormat);
            string base64String;
            if (itemObject is BitmapSource)
            {
                base64String = Convert.ToBase64String(ImageToByteArray((BitmapSource)itemObject));
            }
            else
            {
                base64String = Convert.ToBase64String(ObjectToByteArray(itemObject));
            }
            writer.Write(base64String);
        }

        private void ReadFromStream(BinaryReader reader)
        {
            text = reader.ReadString();
            objectFormat = reader.ReadString();
            byte[] base64Bytes = Convert.FromBase64String(reader.ReadString());
            if (objectFormat == Globals.SBitmap)
            {
                itemObject = ByteArrayToImage(base64Bytes);
            }
            else
            {
                itemObject = ByteArrayToObject(base64Bytes);
            }
        }
    }
}
