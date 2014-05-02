// -----------------------------------------------------------------------
// <copyright file="KinectCursorVisualizer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.Controls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;

    using Microsoft.Kinect.Toolkit.Interaction;

    /// <summary>
    /// Simple WPF Adorner to display Kinect HandPointers
    /// </summary>
    public class KinectCursorVisualizer : Canvas
    {
        public static readonly DependencyProperty CursorPressingColorProperty = DependencyProperty.Register(
                "CursorPressingColor", typeof(Color), typeof(KinectCursorVisualizer), new PropertyMetadata(Color.FromArgb(255, 102, 48, 133)));

        public static readonly DependencyProperty CursorExtendedColor1Property = DependencyProperty.Register(
            "CursorExtendedColor1", typeof(Color), typeof(KinectCursorVisualizer), new PropertyMetadata(Color.FromArgb(255, 1, 179, 255)));

        public static readonly DependencyProperty CursorExtendedColor2Property = DependencyProperty.Register(
            "CursorExtendedColor2", typeof(Color), typeof(KinectCursorVisualizer), new PropertyMetadata(Color.FromArgb(255, 04, 229, 255)));

        public static readonly DependencyProperty CursorGrippedColor1Property = DependencyProperty.Register(
            "CursorGrippedColor1", typeof(Color), typeof(KinectCursorVisualizer), new PropertyMetadata(Color.FromArgb(255, 1, 179, 255)));

        public static readonly DependencyProperty CursorGrippedColor2Property = DependencyProperty.Register(
            "CursorGrippedColor2", typeof(Color), typeof(KinectCursorVisualizer), new PropertyMetadata(Color.FromArgb(255, 04, 229, 255)));

        private const double CursorBoundsMargin = 20.0;

        // check for design mode
        private static readonly bool IsInDesignMode = DesignerProperties.GetIsInDesignMode(new DependencyObject());
        
        private readonly KinectRegionBinder kinectRegionBinder;

        private readonly Dictionary<HandPointer, KinectCursor> pointerCursorMap;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "We need to OverrideMetadata in the static constructor")]
        static KinectCursorVisualizer()
        {
            // Set default style key to be this control type
            DefaultStyleKeyProperty.OverrideMetadata(typeof(KinectCursorVisualizer), new FrameworkPropertyMetadata(typeof(KinectCursorVisualizer)));

            // Set default style to have FlowDirection be LeftToRight
            var style = new Style(typeof(KinectCursorVisualizer), null);
            style.Setters.Add(new Setter(FlowDirectionProperty, FlowDirection.LeftToRight));
            style.Seal();
            StyleProperty.OverrideMetadata(typeof(KinectCursorVisualizer), new FrameworkPropertyMetadata(style));
        }

        public KinectCursorVisualizer()
        {
            this.kinectRegionBinder = new KinectRegionBinder(this);
            this.kinectRegionBinder.OnKinectRegionChanged += this.OnKinectRegionChanged;

            // This makes the adorner ignore all mouse input
            this.IsHitTestVisible = false;

            this.pointerCursorMap = new Dictionary<HandPointer, KinectCursor>();
        }

        public Color CursorPressingColor
        {
            get
            {
                return (Color)this.GetValue(CursorPressingColorProperty);
            }

            set
            {
                this.SetValue(CursorPressingColorProperty, value);
            }
        }

        public Color CursorExtendedColor1
        {
            get
            {
                return (Color)this.GetValue(CursorExtendedColor1Property);
            }

            set
            {
                this.SetValue(CursorExtendedColor1Property, value);
            }
        }

        public Color CursorExtendedColor2
        {
            get
            {
                return (Color)this.GetValue(CursorExtendedColor2Property);
            }

            set
            {
                this.SetValue(CursorExtendedColor2Property, value);
            }
        }

        public Color CursorGrippedColor1
        {
            get
            {
                return (Color)this.GetValue(CursorGrippedColor1Property);
            }

            set
            {
                this.SetValue(CursorGrippedColor1Property, value);
            }
        }

        public Color CursorGrippedColor2
        {
            get
            {
                return (Color)this.GetValue(CursorGrippedColor2Property);
            }

            set
            {
                this.SetValue(CursorGrippedColor2Property, value);
            }
        }

        private static double Clamp(double value, double min, double max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }

        private void OnKinectRegionChanged(object sender, KinectRegion oldRegion, KinectRegion newRegion)
        {
            if (IsInDesignMode)
            {
                return;
            }

            if (oldRegion != null)
            {
                oldRegion.HandPointersUpdated -= this.OnHandPointersUpdated;
            }

            if (newRegion != null)
            {
                newRegion.HandPointersUpdated += this.OnHandPointersUpdated;
            }
        }

        /// <summary>
        /// Update any cursors we are displaying
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="args">Event arguments</param>
        private void OnHandPointersUpdated(object sender, EventArgs args)
        {
            var kinectRegion = KinectRegion.GetKinectRegion(this);
            if (kinectRegion == null)
            {
                return;
            }

            // add the primary hand of the primary user if we need to
            foreach (HandPointer pointer in kinectRegion.HandPointers)
            {
                if (!pointer.IsPrimaryUser || !pointer.IsPrimaryHandOfUser)
                {
                    continue;
                }

                if (!this.pointerCursorMap.ContainsKey(pointer))
                {
                    var cursor = new KinectCursor();
                    cursor.SetBinding(KinectCursor.CursorPressingColorProperty, new Binding("CursorPressingColor") { Source = this });
                    cursor.SetBinding(KinectCursor.CursorExtendedColor1Property, new Binding("CursorExtendedColor1") { Source = this });
                    cursor.SetBinding(KinectCursor.CursorExtendedColor2Property, new Binding("CursorExtendedColor2") { Source = this });
                    cursor.SetBinding(KinectCursor.CursorGrippedColor1Property, new Binding("CursorGrippedColor1") { Source = this });
                    cursor.SetBinding(KinectCursor.CursorGrippedColor2Property, new Binding("CursorGrippedColor2") { Source = this });
                    this.pointerCursorMap[pointer] = cursor;
                    this.Children.Add(cursor);
                }
            }

            // check for deleted ones - either they are not in the
            // KinectRegion's list or they are no longer the primary
            var pointersToRemove = new List<HandPointer>();
            foreach (HandPointer pointer in this.pointerCursorMap.Keys)
            {
                if (!kinectRegion.HandPointers.Contains(pointer) || !pointer.IsPrimaryUser || !pointer.IsPrimaryHandOfUser)
                {
                    pointersToRemove.Add(pointer);
                }
            }

            // delete as needed
            foreach (HandPointer pointer in pointersToRemove)
            {
                this.Children.Remove(this.pointerCursorMap[pointer]);
                this.pointerCursorMap.Remove(pointer);
            }

            // update all current ones
            foreach (HandPointer pointer in this.pointerCursorMap.Keys)
            {
                KinectCursor cursor = this.pointerCursorMap[pointer];

                // Set open state
                cursor.IsOpen = !pointer.IsInGripInteraction;

                // Get information about what this hand pointer is over
                bool isHovering = false;
                bool isOverPressTarget = false;
                foreach (UIElement element in pointer.EnteredElements)
                {
                    if (KinectRegion.GetIsPressTarget(element))
                    {
                        isHovering = true;
                        isOverPressTarget = true;
                        break;
                    }

                    if (KinectRegion.GetIsGripTarget(element))
                    {
                        isHovering = true;
                    }
                }

                // If the cursor is not over anything that considers itself pressable then don't
                // display any pressing progress.
                double adjustedPressExtent = isOverPressTarget ? pointer.PressExtent : 0.0;

                cursor.IsHovering = isHovering;
                cursor.IsPressed = isOverPressTarget && pointer.IsPressed && !pointer.IsInGripInteraction;
                cursor.PressExtent = adjustedPressExtent;

                // pointer.PressExtent has a range of 0..1 - map that to Min/Max for cursor scale
                double finalRadius = KinectCursor.ArtworkSize * (1.0 - (adjustedPressExtent * ((KinectCursor.MaximumCursorScale - KinectCursor.MinimumCursorScale) / 2.0)));

                // Compute Transforms
                double scaleX = finalRadius / KinectCursor.ArtworkSize;
                double scaleY = finalRadius / KinectCursor.ArtworkSize;

                // Flip hand for Left
                if (pointer.HandType == HandType.Left)
                {
                    scaleX *= -1;
                }

                var handScale = new ScaleTransform(scaleX, scaleY);

                // Transform the vector art to match rendering size
                cursor.RenderTransform = handScale;

                double deltaX = (KinectCursor.ArtworkSize / 2) * scaleX;
                double deltaY = (KinectCursor.ArtworkSize / 2) * scaleY;

                // Clamp to KinectRegion bounds
                var cursorCanvasPosition = pointer.GetPosition(null);

                cursorCanvasPosition.X = Clamp(cursorCanvasPosition.X, -CursorBoundsMargin, kinectRegion.ActualWidth + CursorBoundsMargin);
                cursorCanvasPosition.Y = Clamp(cursorCanvasPosition.Y, -CursorBoundsMargin, kinectRegion.ActualHeight + CursorBoundsMargin);

                // If the cursor is not in the interactive area, show the cursor as 70% transparent
                cursor.Opacity = pointer.IsInteractive ? 1.0 : 0.3;

                Canvas.SetLeft(cursor, cursorCanvasPosition.X - deltaX);
                Canvas.SetTop(cursor, cursorCanvasPosition.Y - deltaY);
            }
        }
    }
}
