using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace motion_paint
{
    public static class DrawCanvas
    {

        public static void Paint(Point startPoint, Point nextPoint, InkCanvas Surface, Color color, int thickness)
        {
            Point currentPoint = new Point();
            Line line = new Line();
            if (currentPoint.X == 0 && currentPoint.Y == 0)
            {
                currentPoint = new Point();
                currentPoint = startPoint;
            }
            
            line.Stroke = new SolidColorBrush(color);

            line.StrokeThickness = thickness;
            line.StrokeDashCap = PenLineCap.Round;
            line.StrokeStartLineCap = PenLineCap.Round;
            line.StrokeEndLineCap = PenLineCap.Round;

            line.X1 = currentPoint.X;
            line.Y1 = currentPoint.Y;
            line.X2 = nextPoint.X;
            line.Y2 = nextPoint.Y;

            currentPoint = nextPoint;

            Surface.Children.Add(line);
        }
    }
}
