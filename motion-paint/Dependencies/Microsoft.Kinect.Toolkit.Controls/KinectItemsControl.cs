// -----------------------------------------------------------------------
// <copyright file="KinectItemsControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.Controls
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;

    /// <summary>
    /// An ItemsControl that creates KinectButtons for data bound items and wraps them in a KinectScrollViewer
    /// </summary>
    [TemplatePart(Name = "PART_ScrollViewer", Type = typeof(KinectScrollViewer))]
    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(KinectTileButton))]
    public sealed class KinectItemsControl : ItemsControl
    {
        /// <summary>
        /// Path of the data member to bind to the Label member of the KinectTileButton
        /// </summary>
        public static readonly DependencyProperty LabelMemberPathProperty = DependencyProperty.Register(
            "LabelMemberPath", typeof(string), typeof(KinectItemsControl), new PropertyMetadata(null));

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                "Orientation",
                typeof(Orientation),
                typeof(KinectItemsControl),
                new PropertyMetadata(Orientation.Horizontal));

        public static readonly RoutedEvent ItemClickedEvent = EventManager.RegisterRoutedEvent(
            "ItemClick", RoutingStrategy.Bubble, typeof(EventHandler<KinectItemClickEventArgs>), typeof(KinectItemsControl));

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "We need to OverrideMetadata in the static constructor")]
        static KinectItemsControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(KinectItemsControl),
                new FrameworkPropertyMetadata(typeof(KinectItemsControl)));
        }

        public KinectItemsControl()
        {
            this.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(this.OnItemClicked));
        }

        public event RoutedEventHandler ItemClick
        {
            add { this.AddHandler(ItemClickedEvent, value); }
            remove { this.RemoveHandler(ItemClickedEvent, value); }
        }
        
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { this.SetValue(OrientationProperty, value); }
        }

        /// <summary>
        /// Path of member to bind to the Label of the KinectTileButton
        /// </summary>
        public string LabelMemberPath
        {
            get
            {
                return (string)this.GetValue(LabelMemberPathProperty);
            }

            set
            {
                this.SetValue(LabelMemberPathProperty, value);
            }
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is KinectTileButton;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            var container = new KinectTileButton();

            if (!string.IsNullOrEmpty(this.LabelMemberPath))
            {
                container.SetBinding(KinectTileButton.LabelProperty, new Binding(this.LabelMemberPath));
            }

            return container;
        }

        private void OnItemClicked(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is KinectButtonBase)
            {
                var args = new KinectItemClickEventArgs(ItemClickedEvent, e.OriginalSource);
                this.RaiseEvent(args);
            }
        }
    }
}