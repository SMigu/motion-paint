// -----------------------------------------------------------------------
// <copyright file="DoubleToThicknessConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.Controls
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public class DoubleToThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double left = 0;
            double top = 0;
            double right = 0;
            double bottom = 0;

            double thickness = System.Convert.ToDouble(value, culture);
            string paramString = ((string)parameter).ToUpperInvariant();
            if (paramString.Contains("LEFT"))
            {
                left = thickness;
            }

            if (paramString.Contains("TOP"))
            {
                top = thickness;
            }

            if (paramString.Contains("RIGHT"))
            {
                right = thickness;
            }

            if (paramString.Contains("BOTTOM"))
            {
                bottom = thickness;
            }

            return new Thickness(left, top, right, bottom);
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
