// -----------------------------------------------------------------------
// <copyright file="KinectCursor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.Controls
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Animation;

    /// <summary>
    /// Visualization for a Kinect cursor
    /// </summary>
    internal class KinectCursor : Control
    {
        // Rough bounding square around the open/closed hand models
        public const double ArtworkSize = 80.0;

        // Maximum Cursor Scale
        public const double MaximumCursorScale = 1.0;

        // Minimum Cursor Scale
        public const double MinimumCursorScale = 0.8;

        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
            "IsOpen",
            typeof(bool),
            typeof(KinectCursor),
            new UIPropertyMetadata(true, (o, args) => ((KinectCursor)o).EnsureVisualState()));

        public static readonly DependencyProperty IsHoveringProperty = DependencyProperty.Register(
            "IsHovering",
            typeof(bool),
            typeof(KinectCursor),
            new UIPropertyMetadata(false, (o, args) => ((KinectCursor)o).EnsureVisualState()));

        public static readonly DependencyProperty IsPressedProperty = DependencyProperty.Register(
            "IsPressed",
            typeof(bool),
            typeof(KinectCursor),
            new UIPropertyMetadata(false, (o, args) => ((KinectCursor)o).OnIsPressedChanged()));

        public static readonly DependencyProperty PressExtentProperty = DependencyProperty.Register(
            "PressExtent",
            typeof(double),
            typeof(KinectCursor),
            new UIPropertyMetadata(0.0, (o, args) => ((KinectCursor)o).OnPressExtentChanged()));

        public static readonly DependencyProperty CursorPressingColorProperty = KinectCursorVisualizer.CursorPressingColorProperty.AddOwner(typeof(KinectCursor));

        public static readonly DependencyProperty CursorExtendedColor1Property = KinectCursorVisualizer.CursorExtendedColor1Property.AddOwner(typeof(KinectCursor));

        public static readonly DependencyProperty CursorExtendedColor2Property = KinectCursorVisualizer.CursorExtendedColor2Property.AddOwner(typeof(KinectCursor));

        public static readonly DependencyProperty CursorGrippedColor1Property = KinectCursorVisualizer.CursorGrippedColor1Property.AddOwner(typeof(KinectCursor));

        public static readonly DependencyProperty CursorGrippedColor2Property = KinectCursorVisualizer.CursorGrippedColor2Property.AddOwner(typeof(KinectCursor));


        private FrameworkElement pressStoryboardTarget;

        private Storyboard pressStoryboard;

        private string currentVisualState;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "We need to OverrideMetadata in the static constructor")]
        static KinectCursor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(KinectCursor), new FrameworkPropertyMetadata(typeof(KinectCursor)));
        }

        public KinectCursor()
        {
            this.Width = ArtworkSize;
            this.Height = ArtworkSize;

            this.Loaded += this.KinectCursorLoaded;
        }

        public bool IsOpen 
        { 
            get
            {
                return (bool)this.GetValue(IsOpenProperty);
            }

            set
            {
                this.SetValue(IsOpenProperty, value);
            }
        }

        public bool IsHovering
        { 
            get
            {
                return (bool)GetValue(IsHoveringProperty);
            }

            set
            {
                this.SetValue(IsHoveringProperty, value);
            }
        }

        public bool IsPressed
        {
            get
            {
                return (bool)GetValue(IsPressedProperty);
            }

            set
            {
                this.SetValue(IsPressedProperty, value);
            }
        }

        public double PressExtent
        {
            get
            {
                return (double)this.GetValue(PressExtentProperty);
            }

            set
            {
                this.SetValue(PressExtentProperty, value);
            }
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

        protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
        {
            base.OnTemplateChanged(oldTemplate, newTemplate);

            if (this.pressStoryboard != null)
            {
                this.pressStoryboard.Stop(this.pressStoryboardTarget);
                this.pressStoryboard = null;
            }

            this.currentVisualState = null;
            this.OnIsPressedChanged();
            this.OnPressExtentChanged();
        }

        private void KinectCursorLoaded(object sender, RoutedEventArgs e)
        {
            if (VisualTreeHelper.GetChildrenCount(this) > 0)
            {
                this.pressStoryboardTarget = VisualTreeHelper.GetChild(this, 0) as Grid;
                if (this.pressStoryboardTarget != null)
                {
                    this.pressStoryboard = this.pressStoryboardTarget.TryFindResource("CursorPress") as Storyboard;
                }

                this.currentVisualState = null;
                this.OnIsPressedChanged();
                this.OnPressExtentChanged();
            }
        }

        private void OnIsPressedChanged()
        {
            if (this.pressStoryboard != null)
            {
                if (this.IsPressed)
                {
                    this.pressStoryboard.Remove();
                }
                else
                {
                    this.RestartPressStoryboard();
                }
            }

            this.EnsureVisualState();
        }

        private void EnsureVisualState()
        {
            if (!this.IsOpen)
            {
                this.GoToState("HandClosed");
            }
            else if (this.IsPressed)
            {
                this.GoToState("Pressed");
            }
            else if (this.IsHovering)
            {
                this.GoToState("Hover");
            }
            else
            {
                this.GoToState("Idle");
            }
        }

        private void RestartPressStoryboard()
        {
            if (this.pressStoryboard != null)
            {
                // Put the storyboard in a state where it can Seek
                // arbitrarily
                this.pressStoryboard.Begin(this.pressStoryboardTarget, true);
                this.pressStoryboard.Pause(this.pressStoryboardTarget);
            }
        }
        
        private void OnPressExtentChanged()
        {
            if (this.pressStoryboard != null)
            {
                this.pressStoryboard.Seek(this.pressStoryboardTarget, TimeSpan.FromSeconds(this.PressExtent), TimeSeekOrigin.BeginTime);
            }
        }

        private void GoToState(string newState)
        {
            if (this.currentVisualState != newState)
            {
                this.currentVisualState = newState;
                VisualStateManager.GoToState(this, newState, true);
            }
        }
    }
}
