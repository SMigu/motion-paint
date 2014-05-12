using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace motion_paint
{
    public static class Loadimage
    {

        public static void ImportPng(Uri path, InkCanvas Surface)
        {
            if (path == null) return;

            BitmapSource bitMapSource;

            // Create a file stream for saving image
            using (FileStream inStream = new FileStream(path.LocalPath, FileMode.Open))
            {
                // Use png decoder for png
                PngBitmapDecoder decoder = new PngBitmapDecoder(inStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                bitMapSource = decoder.Frames[0];
            }

            var image = new Image();
            image.Source = bitMapSource;
            Surface.Children.Add(image);
        }
    }
}
