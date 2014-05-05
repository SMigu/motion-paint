﻿using System;
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

        public static void Paint(Point startPoint, Point nextPoint, InkCanvas Surface, Color color, int thickness, string brush)
        {
            Point currentPoint = new Point();
            Line line = new Line();
            if (currentPoint.X == 0 && currentPoint.Y == 0)
            {
                currentPoint = new Point();
                currentPoint = startPoint;
            }
            double angle;
            double radians;
            switch(brush)
            {

                case "brush":            
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
                case "trianglespray":
                    Random NextDouble1 = new Random();
                    int iterationValue1;                   
                    int magnifyValue1;
                    int curveValue1;
                    
                    


                    magnifyValue1 = Convert.ToInt32((NextDouble1.NextDouble() * thickness/3));
                    iterationValue1 = Convert.ToInt32((NextDouble1.NextDouble() * magnifyValue1));
                    for (int i = 0; i < iterationValue1; i++)
                    {
                        startPoint = nextPoint;
                        Polygon Triangle = new Polygon();
                        Point Point1 = new Point(startPoint.X - magnifyValue1, startPoint.Y);
                        Point Point2 = new Point(startPoint.X, startPoint.Y - magnifyValue1);
                        Point Point3 = new Point(startPoint.X + magnifyValue1, startPoint.Y);
                        
                        PointCollection Tripoints = new PointCollection();
                        Tripoints.Add(Point1);
                        Tripoints.Add(Point2);
                        Tripoints.Add(Point3);
                        Triangle.StrokeThickness = (iterationValue1 - i)/3;
                        Triangle.StrokeStartLineCap = PenLineCap.Round;
                        Triangle.StrokeEndLineCap = PenLineCap.Round;
                        Triangle.Points = Tripoints;
                        radians = Math.Atan2(nextPoint.Y - currentPoint.Y, nextPoint.X - currentPoint.X);
                        angle = radians * (180 / Math.PI);

                        Triangle.RenderTransform = new RotateTransform(angle, (nextPoint.X + startPoint.X) / 2, (nextPoint.Y + startPoint.Y) / 2);

                        curveValue1 = Convert.ToInt32(((NextDouble1.NextDouble() * (iterationValue1 - i))) - ((iterationValue1 - i) / 2));
                        nextPoint.X += curveValue1;
                        curveValue1 = Convert.ToInt32(((NextDouble1.NextDouble() * (iterationValue1 - i))) - ((iterationValue1 - i) / 2));
                        nextPoint.Y += curveValue1;

                        //Triangle.Fill = new SolidColorBrush(color);
                        
                        Triangle.Stroke = new SolidColorBrush(color);
                        Surface.Children.Add(Triangle);
                    }
                    break;
                case "starspray":
                    Random NextDouble2 = new Random();
                    int iterationValue2;
                    int magnifyValue2;
                    int curveValue2;
                    
                    magnifyValue2 = Convert.ToInt32((NextDouble2.NextDouble() * thickness/4));
                    iterationValue2 = Convert.ToInt32((NextDouble2.NextDouble() * magnifyValue2));
                    for (int i = 0; i < iterationValue2; i++)
                    {
                        startPoint = nextPoint;
                        Polygon Star = new Polygon();

                        Point Point1 = new Point(startPoint.X - Math.Cos(23*Math.PI/180)*magnifyValue2, startPoint.Y - Math.Sin(23*Math.PI/180)*magnifyValue2);
                        Point Point2 = new Point(startPoint.X + Math.Cos(23 * Math.PI / 180) * magnifyValue2, startPoint.Y - Math.Sin(23 * Math.PI / 180) * magnifyValue2);
                        Point Point3 = new Point(startPoint.X - (Math.Sin(54*Math.PI/180)*(Math.Cos(23*Math.PI/180)-Math.Sin(23*Math.PI/180)/(Math.Tan(54*Math.PI/180))))* magnifyValue2, startPoint.Y + Math.Sqrt(1-Math.Sin(54*Math.PI/180)*(Math.Cos(23*Math.PI/180)-(Math.Sin(23*Math.PI/180)/Math.Tan(54*Math.PI/180))))*magnifyValue2);
                        Point Point4 = new Point(startPoint.X, startPoint.Y - magnifyValue2);
                        Point Point5 = new Point(startPoint.X + (Math.Sin(54 * Math.PI / 180) * (Math.Cos(23 * Math.PI / 180) - Math.Sin(23 * Math.PI / 180) / (Math.Tan(54 * Math.PI / 180)))) * magnifyValue2, startPoint.Y + Math.Sqrt(1 - Math.Sin(54 * Math.PI / 180) * (Math.Cos(23 * Math.PI / 180) - (Math.Sin(23 * Math.PI / 180) / Math.Tan(54 * Math.PI / 180)))) * magnifyValue2);
                        
                       
                        //Point Point1 = new Point(startPoint.X + magnifyValue2, startPoint.Y);
                        //Point Point2 = new Point(startPoint.X - magnifyValue2  , startPoint.Y);
                        //Point Point3 = new Point(startPoint.X + magnifyValue2 * 0.6, startPoint.Y - magnifyValue2);
                        //Point Point4 = new Point(startPoint.X, startPoint.Y + magnifyValue2 * 0.6);
                        //Point Point5 = new Point(startPoint.X - magnifyValue2* 0.6, startPoint.Y - magnifyValue2);
                        //Point Point6 = new Point(startPoint.X + magnifyValue2 * 0.6, startPoint.Y);

                        PointCollection Starpoints = new PointCollection();
                        Starpoints.Add(Point1);
                        Starpoints.Add(Point2);
                        Starpoints.Add(Point3);
                        Starpoints.Add(Point4);
                        Starpoints.Add(Point5);
                       

                        Star.StrokeThickness = (iterationValue2 - i);
                        Star.StrokeStartLineCap = PenLineCap.Round;
                        Star.StrokeEndLineCap = PenLineCap.Round;
                        Star.Points = Starpoints;
                        radians = Math.Atan2(nextPoint.Y - currentPoint.Y, nextPoint.X - currentPoint.X);
                        angle = radians * (180 / Math.PI);

                        Star.RenderTransform = new RotateTransform(angle, (nextPoint.X + startPoint.X) / 2, (nextPoint.Y + startPoint.Y) / 2);

                        curveValue2 = Convert.ToInt32(((NextDouble2.NextDouble() * (iterationValue2 - i))) - ((iterationValue2 - i) / 2));
                        nextPoint.X += curveValue2;
                        curveValue2 = Convert.ToInt32(((NextDouble2.NextDouble() * (iterationValue2 - i))) - ((iterationValue2 - i) / 2));
                        nextPoint.Y += curveValue2;
                        Star.Fill = new SolidColorBrush(color);

                        Star.Stroke = new SolidColorBrush(color);
                        Surface.Children.Add(Star);
                    }
                    break;
                case "squarespray":
                    Random NextDouble3 = new Random();
                    int iterationValue3;
                    int magnifyValue3;
                    int curveValue3;
                    magnifyValue3 = Convert.ToInt32((NextDouble3.NextDouble() * thickness/4));
                    iterationValue3 = Convert.ToInt32((NextDouble3.NextDouble() * magnifyValue3));
                    for (int i = 0; i < iterationValue3; i++)
                    {
                        startPoint = nextPoint;
                        Polygon Square = new Polygon();
                        Point Point1 = new Point(startPoint.X + magnifyValue3/2, startPoint.Y);
                        Point Point2 = new Point(startPoint.X + magnifyValue3/2, startPoint.Y + magnifyValue3/2);
                        Point Point3 = new Point(startPoint.X - magnifyValue3/2, startPoint.Y + magnifyValue3/2);
                        Point Point4 = new Point(startPoint.X - magnifyValue3/2, startPoint.Y - magnifyValue3/2);
                        Point Point5 = new Point(startPoint.X + magnifyValue3/2, startPoint.Y - magnifyValue3/2);
                        

                        PointCollection Squarepoints = new PointCollection();
                        Squarepoints.Add(Point1);
                        Squarepoints.Add(Point2);
                        Squarepoints.Add(Point3);
                        Squarepoints.Add(Point4);
                        Squarepoints.Add(Point5);
                        

                        Square.StrokeThickness = (iterationValue3 - i);
                        Square.StrokeStartLineCap = PenLineCap.Round;
                        Square.StrokeEndLineCap = PenLineCap.Round;
                        Square.Points = Squarepoints;
                        radians = Math.Atan2(nextPoint.Y - currentPoint.Y, nextPoint.X - currentPoint.X);
                        angle = radians * (180 / Math.PI);

                        Square.RenderTransform = new RotateTransform(angle, (nextPoint.X + startPoint.X) / 2, (nextPoint.Y + startPoint.Y) / 2);

                        curveValue3 = Convert.ToInt32(((NextDouble3.NextDouble() * (iterationValue3 - i))) - ((iterationValue3 - i) / 2));
                        nextPoint.X += curveValue3;
                        curveValue3 = Convert.ToInt32(((NextDouble3.NextDouble() * (iterationValue3 - i))) - ((iterationValue3 - i) / 2));
                        nextPoint.Y += curveValue3;
                        //Square.Fill = new SolidColorBrush(color);

                        Square.Stroke = new SolidColorBrush(color);
                        Surface.Children.Add(Square);
                    }
                    break;
                default:
                    break;
                    


            }
        }
    }
}
