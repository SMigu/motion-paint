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

            BitmapSource bitmapSource;

            using (Stream imageStreamSource = new FileStream(path.LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                PngBitmapDecoder decoder = new PngBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                bitmapSource = decoder.Frames[0];
            }
           
            // Draw the Image
            var image = new Image();
            image.Source = bitmapSource;
            image.Stretch = Stretch.UniformToFill;
            Surface.Children.Add(image);
        }
    }
}
