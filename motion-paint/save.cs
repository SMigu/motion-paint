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
    public static class Saveimage
    {

        public static void ExportToPng(Uri path, InkCanvas Surface)
        {
            if (path == null) return;

            // Save current canvas transform
            Transform transform = Surface.LayoutTransform;
            // reset current transform (in case it is scaled or rotated)
            Surface.LayoutTransform = null;

            // Get the size of canvas
            Size size = new Size(Surface.Width, Surface.Height);
            // Measure and arrange the surface
            // VERY IMPORTANT
            Surface.Measure(size);
            Surface.Arrange(new Rect(size));

            // Create a render bitmap and push the surface to it
            RenderTargetBitmap renderBitmap =
              new RenderTargetBitmap(
                (int)size.Width,
                (int)size.Height,
                96d,
                96d,
                PixelFormats.Pbgra32);
            renderBitmap.Render(Surface);

            // Create a file stream for saving image
            using (FileStream outStream = new FileStream(path.LocalPath, FileMode.Create))
            {
                // Use png encoder for our data
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                // push the rendered bitmap to it
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                // save the data to the stream
                encoder.Save(outStream);
            }

            // Restore previously saved layout
            Surface.LayoutTransform = transform;
        }
    }
}
