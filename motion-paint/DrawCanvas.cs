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
            double angle;
            double radians;
            Random NextDouble = new Random();
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
                    //Random NextDouble = new Random();
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
                    int iterationValue1;                   
                    int magnifyValue1;
                    //int curveValue1;  
                    if (thickness<20)
                    {
                        thickness = 20;
                    }

                    magnifyValue1 = Convert.ToInt32((NextDouble.NextDouble() * thickness)*1.5);
                    iterationValue1 = Convert.ToInt32((NextDouble.NextDouble() * magnifyValue1)/2);
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
                        Triangle.StrokeThickness = (iterationValue1 - i)/8;
                        Triangle.StrokeStartLineCap = PenLineCap.Round;
                        Triangle.StrokeEndLineCap = PenLineCap.Round;
                        Triangle.Points = Tripoints;
                        radians = Math.Atan2(nextPoint.Y - currentPoint.Y, nextPoint.X - currentPoint.X);
                        angle = (radians) * (180 / Math.PI)*1.4;

                        Triangle.RenderTransform = new RotateTransform(angle, (nextPoint.X + startPoint.X) / 2, (nextPoint.Y + startPoint.Y) / 2);


                        //curveValue1 = Convert.ToInt32(((NextDouble1.NextDouble() * (iterationValue1 - i))) - ((iterationValue1 - i) / 2));
                        //nextPoint.X += curveValue1/4;
                        //curveValue1 = Convert.ToInt32(((NextDouble1.NextDouble() * (iterationValue1 - i))) - ((iterationValue1 - i) / 2));
                        //nextPoint.Y += curveValue1/4;


                        //Triangle.Fill = new SolidColorBrush(color);
                        
                        Triangle.Stroke = new SolidColorBrush(color);
                        Surface.Children.Add(Triangle);
                    }
                    break;
                case "starspray":
                    int iterationValue2;
                    int magnifyValue2;
                    //int curveValue2;
                    if (thickness < 20)
                    {
                        thickness = 20;
                    }
                    
                    magnifyValue2 = Convert.ToInt32((NextDouble.NextDouble() * thickness));
                    iterationValue2 = Convert.ToInt32((NextDouble.NextDouble() * magnifyValue2)/5);
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
                       

                        Star.StrokeThickness = 1;
                        Star.StrokeStartLineCap = PenLineCap.Round;
                        Star.StrokeEndLineCap = PenLineCap.Round;
                        Star.Points = Starpoints;
                        radians = Math.Atan2(nextPoint.Y - currentPoint.Y, nextPoint.X - currentPoint.X);
                        angle = radians * (180 / Math.PI) + magnifyValue2;

                        Star.RenderTransform = new RotateTransform(angle, (nextPoint.X + startPoint.X) / 2, (nextPoint.Y + startPoint.Y) / 2);

                        //curveValue2 = Convert.ToInt32(((NextDouble2.NextDouble() * (iterationValue2 - i))) - ((iterationValue2 - i) / 2));
                        //nextPoint.X += curveValue2/10;
                        //curveValue2 = Convert.ToInt32(((NextDouble2.NextDouble() * (iterationValue2 - i))) - ((iterationValue2 - i) / 2));
                        //nextPoint.Y += curveValue2/10;
                        Star.Fill = new SolidColorBrush(color);

                        Star.Stroke = new SolidColorBrush(color);
                        Surface.Children.Add(Star);
                    }
                    break;
                case "squarespray":                    
                    int iterationValue3;
                    int magnifyValue3;
                    //int curveValue3;
                    if (thickness < 20)
                    {
                        thickness = 20;
                    }
                    magnifyValue3 = Convert.ToInt32((NextDouble.NextDouble() * thickness)*1.5);
                    iterationValue3 = Convert.ToInt32((NextDouble.NextDouble() * magnifyValue3)/1.5);
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
                        

                        Square.StrokeThickness = (iterationValue3 - i)/8;
                        Square.StrokeStartLineCap = PenLineCap.Round;
                        Square.StrokeEndLineCap = PenLineCap.Round;
                        Square.Points = Squarepoints;
                        radians = Math.Atan2(nextPoint.Y - currentPoint.Y, nextPoint.X - currentPoint.X);
                        angle = radians * (180 / Math.PI)*1.3;

                        Square.RenderTransform = new RotateTransform(angle, (nextPoint.X + startPoint.X) / 2, (nextPoint.Y + startPoint.Y) / 2);
                        
                        //curveValue3 = Convert.ToInt32(((NextDouble3.NextDouble() * (iterationValue3 - i))) - ((iterationValue3 - i) / 2));
                        //nextPoint.X += curveValue3;
                        //curveValue3 = Convert.ToInt32(((NextDouble3.NextDouble() * (iterationValue3 - i))) - ((iterationValue3 - i) / 2));
                        //nextPoint.Y += curveValue3;
                        //Square.Fill = new SolidColorBrush(color);

                        Square.Stroke = new SolidColorBrush(color);
                        Surface.Children.Add(Square);
                    }
                    break;
                case "throwpaint":

                    while (nextDepth.Z - startDepth.Z > 0.05)
                    {
                        
                        Point modPoint = new Point();

                        modPoint.X = nextPoint.X + (nextPoint.X - startPoint.X)*2;
                        modPoint.Y = nextPoint.Y + (nextPoint.Y - startPoint.Y)*2;
                        Point mPoint1 = new Point((startPoint.X + nextPoint.X) / 2, (startPoint.Y + nextPoint.Y) / 2);
                        Point mPoint3 = new Point((startPoint.X + mPoint1.X) / 2, (startPoint.Y + mPoint1.Y) / 2);
                        Point mPoint2 = new Point((nextPoint.X + modPoint.X) / 2, (nextPoint.Y + modPoint.Y) / 2);
                        Point mPoint4 = new Point((mPoint1.X + nextPoint.X) / 2, (mPoint1.Y + nextPoint.Y) / 2);
                        Point mPoint5 = new Point((nextPoint.X + mPoint2.X) / 2, (nextPoint.Y + mPoint2.Y) / 2);
                        Point mPoint6 = new Point((mPoint2.X + modPoint.X) / 2, (mPoint2.Y + modPoint.Y) / 2);
                        
                        nextDepth.Z = startDepth.Z;
                        

                        int iterationValue4;
                        int magnifyValue4;
                        magnifyValue4 = Convert.ToInt32((NextDouble.NextDouble() * thickness));
                        iterationValue4 = Convert.ToInt32((NextDouble.NextDouble() * magnifyValue4*2.1));

                        for (int i = 0; i < iterationValue4; i++)
                        {

                        //startPoint = nextPoint;
                        Line Line1 = new Line();
                        Line1.StrokeThickness = (iterationValue4 - i);
                        Line1.StrokeStartLineCap = PenLineCap.Round;
                        Line1.StrokeEndLineCap = PenLineCap.Round;
                        Line1.X1 = startPoint.X;
                        Line1.Y1 = startPoint.Y;
                        curveValue = Convert.ToInt32(((NextDouble.NextDouble() * (iterationValue4 - i))) - ((iterationValue4 - i) / 2));
                        startPoint.X += curveValue;
                        curveValue = Convert.ToInt32(((NextDouble.NextDouble() * (iterationValue4 - i))) - ((iterationValue4 - i) / 2));
                        startPoint.Y += curveValue;
                        Line1.X2 = startPoint.X;
                        Line1.Y2 = startPoint.Y;
                        Line1.Stroke = new SolidColorBrush(color);
                       
                        Surface.Children.Add(Line1);

                        Line Line6 = new Line();
                        
                        Line6.StrokeThickness = (iterationValue4 - i);
                        Line6.StrokeStartLineCap = PenLineCap.Round;
                        Line6.StrokeEndLineCap = PenLineCap.Round;
                        Line6.X1 = mPoint3.X;
                        Line6.Y1 = mPoint3.Y;
                        curveValue = Convert.ToInt32(((NextDouble.NextDouble() * (iterationValue4 - i))) - ((iterationValue4 - i) / 2));
                        mPoint3.X += curveValue;
                        curveValue = Convert.ToInt32(((NextDouble.NextDouble() * (iterationValue4 - i))) - ((iterationValue4 - i) / 2));
                        mPoint3.Y += curveValue;
                        Line6.X2 = mPoint3.X;
                        Line6.Y2 = mPoint3.Y;
                        Line6.Stroke = new SolidColorBrush(color);

                        Surface.Children.Add(Line6);

                        Line Line4 = new Line();
                        
                        Line4.StrokeThickness = (iterationValue4 - i);
                        Line4.StrokeStartLineCap = PenLineCap.Round;
                        Line4.StrokeEndLineCap = PenLineCap.Round;
                        Line4.X1 = mPoint1.X;
                        Line4.Y1 = mPoint1.Y;
                        curveValue = Convert.ToInt32(((NextDouble.NextDouble() * (iterationValue4 - i))) - ((iterationValue4 - i) / 2));
                        mPoint1.X += curveValue;
                        curveValue = Convert.ToInt32(((NextDouble.NextDouble() * (iterationValue4 - i))) - ((iterationValue4 - i) / 2));
                        mPoint1.Y += curveValue;
                        Line4.X2 = mPoint1.X;
                        Line4.Y2 = mPoint1.Y;
                        Line4.Stroke = new SolidColorBrush(color);

                        Surface.Children.Add(Line4);

                        Line Line7 = new Line();

                        Line7.StrokeThickness = (iterationValue4 - i);
                        Line7.StrokeStartLineCap = PenLineCap.Round;
                        Line7.StrokeEndLineCap = PenLineCap.Round;
                        Line7.X1 = mPoint4.X;
                        Line7.Y1 = mPoint4.Y;
                        curveValue = Convert.ToInt32(((NextDouble.NextDouble() * (iterationValue4 - i))) - ((iterationValue4 - i) / 2));
                        mPoint4.X += curveValue;
                        curveValue = Convert.ToInt32(((NextDouble.NextDouble() * (iterationValue4 - i))) - ((iterationValue4 - i) / 2));
                        mPoint4.Y += curveValue;
                        Line7.X2 = mPoint4.X;
                        Line7.Y2 = mPoint4.Y;
                        Line7.Stroke = new SolidColorBrush(color);

                        Surface.Children.Add(Line7);

                        Line Line8 = new Line();

                        Line8.StrokeThickness = (iterationValue4 - i);
                        Line8.StrokeStartLineCap = PenLineCap.Round;
                        Line8.StrokeEndLineCap = PenLineCap.Round;
                        Line8.X1 = mPoint5.X;
                        Line8.Y1 = mPoint5.Y;
                        curveValue = Convert.ToInt32(((NextDouble.NextDouble() * (iterationValue4 - i))) - ((iterationValue4 - i) / 2));
                        mPoint5.X += curveValue;
                        curveValue = Convert.ToInt32(((NextDouble.NextDouble() * (iterationValue4 - i))) - ((iterationValue4 - i) / 2));
                        mPoint5.Y += curveValue;
                        Line8.X2 = mPoint5.X;
                        Line8.Y2 = mPoint5.Y;
                        Line8.Stroke = new SolidColorBrush(color);

                        Surface.Children.Add(Line8);

                        Line Line9 = new Line();

                        Line9.StrokeThickness = (iterationValue4 - i);
                        Line9.StrokeStartLineCap = PenLineCap.Round;
                        Line9.StrokeEndLineCap = PenLineCap.Round;
                        Line9.X1 = mPoint6.X;
                        Line9.Y1 = mPoint6.Y;
                        curveValue = Convert.ToInt32(((NextDouble.NextDouble() * (iterationValue4 - i))) - ((iterationValue4 - i) / 2));
                        mPoint6.X += curveValue;
                        curveValue = Convert.ToInt32(((NextDouble.NextDouble() * (iterationValue4 - i))) - ((iterationValue4 - i) / 2));
                        mPoint6.Y += curveValue;
                        Line9.X2 = mPoint6.X;
                        Line9.Y2 = mPoint6.Y;
                        Line9.Stroke = new SolidColorBrush(color);

                        Surface.Children.Add(Line9);

                        startPoint = nextPoint;
                        Line Line3 = new Line();
                        Line3.StrokeThickness = (iterationValue4 - i);
                        Line3.StrokeStartLineCap = PenLineCap.Round;
                        Line3.StrokeEndLineCap = PenLineCap.Round;
                        Line3.X1 = nextPoint.X;
                        Line3.Y1 = nextPoint.Y;
                        curveValue = Convert.ToInt32(((NextDouble.NextDouble() * (iterationValue4 - i))) - ((iterationValue4 - i) / 2));
                        nextPoint.X += curveValue;
                        curveValue = Convert.ToInt32(((NextDouble.NextDouble() * (iterationValue4 - i))) - ((iterationValue4 - i) / 2));
                        nextPoint.Y += curveValue;
                        Line3.X2 = nextPoint.X;
                        Line3.Y2 = nextPoint.Y;
                        Line3.Stroke = new SolidColorBrush(color);
                        Surface.Children.Add(Line3);

                        Line Line5 = new Line();
                        
                        Line5.StrokeThickness = (iterationValue4 - i);
                        Line5.StrokeStartLineCap = PenLineCap.Round;
                        Line5.StrokeEndLineCap = PenLineCap.Round;
                        Line5.X1 = mPoint2.X;
                        Line5.Y1 = mPoint2.Y;
                        curveValue = Convert.ToInt32(((NextDouble.NextDouble() * (iterationValue4 - i))) - ((iterationValue4 - i) / 2));
                        mPoint2.X += curveValue;
                        curveValue = Convert.ToInt32(((NextDouble.NextDouble() * (iterationValue4 - i))) - ((iterationValue4 - i) / 2));
                        mPoint2.Y += curveValue;
                        Line5.X2 = mPoint2.X;
                        Line5.Y2 = mPoint2.Y;
                        Line5.Stroke = new SolidColorBrush(color);

                        Surface.Children.Add(Line5);

                        Line Line2 = new Line();                                                
                        Line2.StrokeThickness = (iterationValue4 - i);
                        Line2.StrokeStartLineCap = PenLineCap.Round;
                        Line2.StrokeEndLineCap = PenLineCap.Round;
                        Line2.X1 = modPoint.X;
                        Line2.Y1 = modPoint.Y;
                        curveValue = Convert.ToInt32(((NextDouble.NextDouble() * (iterationValue4 - i))) - ((iterationValue4 - i) / 2));
                        modPoint.X += curveValue;
                        curveValue = Convert.ToInt32(((NextDouble.NextDouble() * (iterationValue4 - i))) - ((iterationValue4 - i) / 2));
                        modPoint.Y += curveValue;
                        Line2.X2 = modPoint.X;
                        Line2.Y2 = modPoint.Y;
                        Line2.Stroke = new SolidColorBrush(color);
                        Surface.Children.Add(Line2);
                        
                        

                        
                        
                        

                            
                        }


                        

                        //line.RenderTransform = new RotateTransform(angle, (nextPoint.X + startPoint.X) / 2, (nextPoint.Y + startPoint.Y) / 2);
                        

                        //currentPoint = modPoint;
                        //currentPoint = modPoint;

                        break;
                   
                    }
                    break;
                default:
                    break;
                    


            }
        }
    }
}
