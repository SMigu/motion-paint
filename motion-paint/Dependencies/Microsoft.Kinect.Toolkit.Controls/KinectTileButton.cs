// -----------------------------------------------------------------------
// <copyright file="KinectTileButton.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.Controls
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    /// <summary>
    /// Button that responds to Kinect events
    /// </summary>
    public class KinectTileButton : KinectButtonBase
    {
        /// <summary>
        /// Horizontal alignment of the Label content
        /// </summary>
        public static readonly DependencyProperty HorizontalLabelAlignmentProperty = DependencyProperty.Register(
            "HorizontalLabelAlignment", typeof(HorizontalAlignment), typeof(KinectTileButton), new PropertyMetadata(HorizontalAlignment.Left));

        /// <summary>
        /// Vertical alignment of the Label content
        /// </summary>
        public static readonly DependencyProperty VerticalLabelAlignmentProperty = DependencyProperty.Register(
            "VerticalLabelAlignment", typeof(VerticalAlignment), typeof(KinectTileButton), new PropertyMetadata(VerticalAlignment.Bottom));

        /// <summary>
        /// The Label content
        /// </summary>
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            "Label", typeof(object), typeof(KinectTileButton), new PropertyMetadata(null));

        /// <summary>
        /// Background of the Label content
        /// </summary>
        public static readonly DependencyProperty LabelBackgroundProperty = DependencyProperty.Register(
            "LabelBackground", typeof(Brush), typeof(KinectTileButton), new PropertyMetadata(null));

        /// <summary>
        /// ContentTemplate for the Label content
        /// </summary>
        public static readonly DependencyProperty LabelTemplateProperty = DependencyProperty.Register(
            "LabelTemplate", typeof(DataTemplate), typeof(KinectTileButton), new PropertyMetadata(null));

        /// <summary>
        /// DataTemplateSelector for the Label content
        /// </summary>
        public static readonly DependencyProperty LabelTemplateSelectorProperty = DependencyProperty.Register(
            "LabelTemplateSelector", typeof(DataTemplateSelector), typeof(KinectTileButton), new PropertyMetadata(null));

        /// <summary>
        /// Initializes static members of the <see cref="KinectTileButton"/> class. 
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "We need to OverrideMetadata in the static constructor")]
        static KinectTileButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(KinectTileButton), new FrameworkPropertyMetadata(typeof(KinectTileButton)));
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
    }
}
