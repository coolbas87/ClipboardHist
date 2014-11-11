using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ClipboardHistory
{
    public static class Utils
    {
        internal static bool IsStringCollectionsEqual(StringCollection col1, StringCollection col2)
        {
            List<string> list1 = col1.Cast<string>().ToList();
            List<string> list2 = col2.Cast<string>().ToList();
            List<string> resList = list1.Except(list2).ToList();
            return resList.Count == 0;
        }

        internal static bool IsImagesEqual(BitmapSource img1, BitmapSource img2)
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
    }
}
