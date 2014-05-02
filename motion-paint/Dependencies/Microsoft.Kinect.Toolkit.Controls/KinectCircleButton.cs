// -----------------------------------------------------------------------
// <copyright file="KinectCircleButton.cs" company="Microsoft">
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

    /// <summary>
    /// Location of the label in relation to the button
    /// </summary>
    public enum KinectCircleButtonLabelPosition
    {
        None = 0,

        Bottom,

        Right,
    }

    /// <summary>
    /// Button that responds to Kinect events
    /// </summary>
    [TemplatePart(Name = "PART_DefaultContentPresenter", Type = typeof(ContentPresenter))]
    public class KinectCircleButton : KinectButtonBase
    {
        /// <summary>
        /// Horizontal alignment of the Label content
        /// </summary>
        public static readonly DependencyProperty HorizontalLabelAlignmentProperty = DependencyProperty.Register(
            "HorizontalLabelAlignment",
            typeof(HorizontalAlignment),
            typeof(KinectCircleButton),
            new PropertyMetadata(HorizontalAlignment.Left));

        /// <summary>
        /// Vertical alignment of the Label content
        /// </summary>
        public static readonly DependencyProperty VerticalLabelAlignmentProperty = DependencyProperty.Register(
            "VerticalLabelAlignment",
            typeof(VerticalAlignment),
            typeof(KinectCircleButton),
            new PropertyMetadata(VerticalAlignment.Bottom));

        /// <summary>
        /// The Label content
        /// </summary>
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            "Label", typeof(object), typeof(KinectCircleButton), new PropertyMetadata(null));

        /// <summary>
        /// Background of the Label content
        /// </summary>
        public static readonly DependencyProperty LabelBackgroundProperty = DependencyProperty.Register(
            "LabelBackground", typeof(Brush), typeof(KinectCircleButton), new PropertyMetadata(null));

        /// <summary>
        /// ContentTemplate for the Label content
        /// </summary>
        public static readonly DependencyProperty LabelTemplateProperty = DependencyProperty.Register(
            "LabelTemplate", typeof(DataTemplate), typeof(KinectCircleButton), new PropertyMetadata(null));

        /// <summary>
        /// DataTemplateSelector for the Label content
        /// </summary>
        public static readonly DependencyProperty LabelTemplateSelectorProperty = DependencyProperty.Register(
            "LabelTemplateSelector",
            typeof(DataTemplateSelector),
            typeof(KinectCircleButton),
            new PropertyMetadata(null));

        /// <summary>
        /// Position of the label
        /// </summary>
        public static readonly DependencyProperty LabelPositionProperty =
            DependencyProperty.Register("LabelPosition", typeof(KinectCircleButtonLabelPosition), typeof(KinectCircleButton), new PropertyMetadata(KinectCircleButtonLabelPosition.Bottom));

        /// <summary>
        /// Foreground brush to use for content (e.g.: arrow glyph inside button)
        /// </summary>
        /// <remarks>
        /// This property is provided as a convenience for content to be able to bind
        /// to it. The expectation is that KinectCircleButton style authors will set
        /// this property in response to control property changes or other triggers.
        /// Color scheme configuration should be done by specifying Foreground and
        /// ContentPressedForeground brushes rather than by explicitly specifying a
        /// ContentForeground brush.
        /// </remarks>
        public static readonly DependencyProperty ContentForegroundProperty = DependencyProperty.Register(
            "ContentForeground", typeof(Brush), typeof(KinectCircleButton), new PropertyMetadata(null));

        /// <summary>
        /// Foreground brush to use for content (e.g.: arrow glyph inside button) when
        /// button is in the pressed state
        /// </summary>
        public static readonly DependencyProperty ContentPressedForegroundProperty = DependencyProperty.Register(
            "ContentPressedForeground", typeof(Brush), typeof(KinectCircleButton), new PropertyMetadata(null));
        
        /// <summary>
        /// Initializes static members of the <see cref="KinectCircleButton"/> class. 
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline",
            Justification = "We need to OverrideMetadata in the static constructor")]
        static KinectCircleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(KinectCircleButton),
                new FrameworkPropertyMetadata(typeof(KinectCircleButton)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KinectCircleButton"/> class. 
        /// </summary>
        public KinectCircleButton()
        {
            this.LayoutUpdated += this.OnLayoutUpdated;
        }

        /// <summary>
        /// Horizontal alignment of the label content
        /// </summary>
        public HorizontalAlignment HorizontalLabelAlignment
        {
            get
            {
                return (HorizontalAlignment)this.GetValue(HorizontalLabelAlignmentProperty);
            }

            set
            {
                this.SetValue(HorizontalLabelAlignmentProperty, value);
            }
        }

        /// <summary>
        /// Vertical alignment of the label content
        /// </summary>
        public VerticalAlignment VerticalLabelAlignment
        {
            get
            {
                return (VerticalAlignment)this.GetValue(VerticalLabelAlignmentProperty);
            }

            set
            {
                this.SetValue(VerticalLabelAlignmentProperty, value);
            }
        }

        /// <summary>
        /// The Label content
        /// </summary>
        public object Label
        {
            get
            {
                return this.GetValue(LabelProperty);
            }

            set
            {
                this.SetValue(LabelProperty, value);
            }
        }

        /// <summary>
        /// Background of the label content
        /// </summary>
        public Brush LabelBackground
        {
            get
            {
                return (Brush)this.GetValue(LabelBackgroundProperty);
            }

            set
            {
                this.SetValue(LabelBackgroundProperty, value);
            }
        }

        /// <summary>
        /// ContentTemplate for the Label content
        /// </summary>
        public DataTemplate LabelTemplate
        {
            get
            {
                return (DataTemplate)this.GetValue(LabelTemplateProperty);
            }

            set
            {
                this.SetValue(LabelTemplateProperty, value);
            }
        }

        /// <summary>
        /// DataTemplateSelector for the Label content
        /// </summary>
        public DataTemplateSelector LabelTemplateSelector
        {
            get
            {
                return (DataTemplateSelector)this.GetValue(LabelTemplateSelectorProperty);
            }

            set
            {
                this.SetValue(LabelTemplateSelectorProperty, value);
            }
        }
        
        /// <summary>
        /// Foreground brush to use for content (e.g.: arrow glyph inside button)
        /// </summary>
        /// <remarks>
        /// This property is provided as a convenience for content to be able to bind
        /// to it. The expectation is that KinectCircleButton style authors will set
        /// this property in response to control property changes or other triggers.
        /// Color scheme configuration should be done by specifying Foreground and
        /// ContentPressedForeground brushes rather than by explicitly specifying a
        /// ContentForeground brush.
        /// </remarks>
        public Brush ContentForeground
        {
            get { return (Brush)this.GetValue(ContentForegroundProperty); }
            set { this.SetValue(ContentForegroundProperty, value); }
        }

        /// <summary>
        /// Foreground brush to use for content (e.g.: arrow glyph inside button) when
        /// button is in the pressed state
        /// </summary>
        public Brush ContentPressedForeground
        {
            get { return (Brush)this.GetValue(ContentPressedForegroundProperty); }
            set { this.SetValue(ContentPressedForegroundProperty, value); }
        }

        /// <summary>
        /// The position of the label in the button
        /// </summary>
        public KinectCircleButtonLabelPosition LabelPosition
        {
            get { return (KinectCircleButtonLabelPosition)this.GetValue(LabelPositionProperty); }
            set { this.SetValue(LabelPositionProperty, value); }
        }

        /// <summary>
        /// Called when the layout updates.  Updates the point that
        /// we guide the hand pointer to when we are being pressed.
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="args">Arguments of the event</param>
        private void OnLayoutUpdated(object sender, EventArgs args)
        {
            var contentPresenter = this.Template.FindName("PART_DefaultContentPresenter", this) as ContentPresenter;
            if (contentPresenter == null)
            {
                return;
            }

            var contentCenterPoint = new Point(contentPresenter.ActualWidth / 2.0, contentPresenter.ActualHeight / 2.0);
            var pressTargetPoint = contentPresenter.TranslatePoint(contentCenterPoint, this);
            pressTargetPoint = new Point(pressTargetPoint.X / this.ActualWidth, pressTargetPoint.Y / this.ActualHeight);
            KinectRegion.SetPressTargetPoint(this, pressTargetPoint);
        }
    }
}