using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace motion_paint
{
    public static class DrawCanvas
    {

        public static void Paint(Point startPoint, Point nextPoint, InkCanvas Surface, Color color, int thickness, string brush, Point3D startDepth, Point3D nextDepth)
        {
            Point currentPoint = new Point();
            Line line = new Line();
            if (currentPoint.X == 0 && currentPoint.Y == 0)
            {
                currentPoint = new Point();
                currentPoint = startPoint;
            }
            switch(brush)
            {

                case "pencil":            
                    //Point currentPoint = new Point();
                    //Line line = new Line();
                    //if (currentPoint.X == 0 && currentPoint.Y == 0)
                    //{
                    //    currentPoint = new Point();
                    //    currentPoint = startPoint;
                    //}
            
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
                    break;
                case "paintsplatter":
                    Random NextDouble = new Random();
                    int iterationValue;
                    int curveValue;
                    int magnifyValue;
                    magnifyValue = Convert.ToInt32((NextDouble.NextDouble() * thickness));
                    iterationValue = Convert.ToInt32((NextDouble.NextDouble() * magnifyValue));
                    for (int i = 0; i < iterationValue; i++)
                    {
                        startPoint = nextPoint;
                        Line Line1 = new Line();
                        Line1.StrokeThickness = (iterationValue - i);
                        Line1.StrokeStartLineCap = PenLineCap.Round;
                        Line1.StrokeEndLineCap = PenLineCap.Round;
                        Line1.X1 = startPoint.X;
                        Line1.Y1 = startPoint.Y;
                        curveValue = Convert.ToInt32(((NextDouble.NextDouble() * (iterationValue - i))) - ((iterationValue - i) / 2));
                        nextPoint.X += curveValue;
                        curveValue = Convert.ToInt32(((NextDouble.NextDouble() * (iterationValue - i))) - ((iterationValue - i) / 2));
                        nextPoint.Y += curveValue;
                        Line1.X2 = nextPoint.X;
                        Line1.Y2 = nextPoint.Y;
                        Line1.Stroke = new SolidColorBrush(color);
                        Surface.Children.Add(Line1);
                    }
                    break;
                case "throwpaint":

                    if (startDepth.Z + nextDepth.Z < -2)
                    {
                        double a;
                        double angle;
                        double radians;
                        
                        Ellipse Ellipse = new Ellipse();
                        a = startDepth.Z - nextDepth.Z;
                        Point modPoint = new Point(nextPoint.X + (nextPoint.X * a)/10, nextPoint.Y + (nextPoint.Y * a)/10);
                        radians = Math.Atan2(modPoint.Y - currentPoint.Y, modPoint.X - currentPoint.X);
                        angle = radians * (180 / Math.PI);
                        Ellipse.Stroke = new SolidColorBrush(color);

                        Ellipse.StrokeThickness = thickness;
                        Ellipse.StrokeDashCap = PenLineCap.Round;
                        Ellipse.StrokeStartLineCap = PenLineCap.Round;
                        Ellipse.StrokeEndLineCap = PenLineCap.Round;

                        InkCanvas.SetBottom(Ellipse, currentPoint.Y);
                        Ellipse.Height = currentPoint.Y - modPoint.Y;
                        Ellipse.Width = thickness*10;
                        Ellipse.RenderTransform = new RotateTransform(angle, Ellipse.Width / 2, Ellipse.Height / 2);
                    


                        currentPoint = nextPoint;

                        Surface.Children.Add(Ellipse);


                    }


                    break;
                default:
                    break;
                    


            }
        }
    }
}
