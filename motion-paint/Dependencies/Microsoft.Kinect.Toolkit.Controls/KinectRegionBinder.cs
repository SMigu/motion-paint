// -----------------------------------------------------------------------
// <copyright file="KinectRegionBinder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.Controls
{
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// Class to help bind KinectRegion to dependency objects.
    /// </summary>
    internal class KinectRegionBinder : DependencyObject
    {
        /// <summary>
        /// KinectRegion dependency property.
        /// </summary>
        public static readonly DependencyProperty KinectRegionProperty = DependencyProperty.Register(
            "KinectRegion",
            typeof(KinectRegion),
            typeof(KinectRegionBinder),
            new PropertyMetadata(null, (o, e) => ((KinectRegionBinder)o).OnKinectRegionPropertyChanged(e.OldValue as KinectRegion, e.NewValue as KinectRegion)));

        /// <summary>
        /// KinectSensor dependency property.
        /// </summary>
        public static readonly DependencyProperty KinectSensorProperty = DependencyProperty.Register(
            "KinectSensor",
            typeof(KinectSensor),
            typeof(KinectRegionBinder),
            new PropertyMetadata(null, (o, e) => ((KinectRegionBinder)o).OnKinectSensorPropertyChanged(e.OldValue as KinectSensor, e.NewValue as KinectSensor)));

        /// <summary>
        /// Initializes a new instance of the <see cref="KinectRegionBinder"/> class. 
        /// </summary>
        /// <param name="source">
        /// The DependencyObject source of the binding.
        /// </param>
        public KinectRegionBinder(DependencyObject source)
        {
            var binding = new Binding { Source = source, Path = new PropertyPath(KinectRegion.KinectRegionProperty) };
            BindingOperations.SetBinding(this, KinectRegionProperty, binding);
        }

        /// <summary>
        /// Handler delegate to KinectRegionChanged event.
        /// </summary>
        public delegate void KinectRegionChangedHandler(object sender, KinectRegion oldValue, KinectRegion newValue);

        /// <summary>
        /// Handler delegate to KinectSensorChanged event.
        /// </summary>
        public delegate void KinectSensorChangedHandler(object sender, KinectSensor oldValue, KinectSensor newValue);

        /// <summary>
        /// KinectRegionChanged event.
        /// </summary>
        public event KinectRegionChangedHandler OnKinectRegionChanged;

        /// <summary>
        /// KinectSensorChanged event.
        /// </summary>
        public event KinectSensorChangedHandler OnKinectSensorChanged;

        public KinectRegion KinectRegion
        {
            get { return (KinectRegion)GetValue(KinectRegionProperty); }
            set { this.SetValue(KinectRegionProperty, value); }
        }

        public KinectSensor KinectSensor
        {
            get { return (KinectSensor)this.GetValue(KinectSensorProperty); }
            set { this.SetValue(KinectSensorProperty, value); }
        }

        /// <summary>
        /// KinectRegion dependency property changed handler.
        /// </summary>
        private void OnKinectRegionPropertyChanged(KinectRegion oldRegion, KinectRegion newRegion)
        {
            if (oldRegion != null)
            {
                BindingOperations.ClearBinding(oldRegion, KinectSensorProperty);
            }

            if (newRegion != null)
            {
                var binding = new Binding("KinectSensor") { Source = newRegion };
                BindingOperations.SetBinding(this, KinectSensorProperty, binding);
            }

            if (this.OnKinectRegionChanged != null)
            {
                this.OnKinectRegionChanged(this, oldRegion, newRegion);
            }
        }

        /// <summary>
        /// KinectSensor dependency property changed handler.
        /// </summary>
        private void OnKinectSensorPropertyChanged(KinectSensor oldSensor, KinectSensor newSensor)
        {
            if (this.OnKinectSensorChanged != null)
            {
                this.OnKinectSensorChanged(this, oldSensor, newSensor);
            }
        }
    }
}
