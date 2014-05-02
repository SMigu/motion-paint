// -----------------------------------------------------------------------
// <copyright file="KinectRegion.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    using Microsoft.Kinect.Toolkit.Interaction;


    /// <summary>
    /// Callback delegate used to ask client who the primary user should be.
    /// </summary>
    /// <param name="proposedTrackingId">
    /// Tracking Id of proposed primary user.
    /// </param>
    /// <param name="candidateHandPointers">
    /// Collection of information about hand pointers from which client can
    /// choose a primary user.
    /// </param>
    /// <param name="timestamp">
    /// Time when delegate was called. Corresponds to InteractionStream and
    /// KinectSensor event timestamps.
    /// </param>
    /// <returns>
    /// The tracking Id of chosen primary user. 0 means that no user should be considered primary.
    /// </returns>
    public delegate int QueryPrimaryUserTrackingIdCallback(int proposedTrackingId, IEnumerable<HandPointer> candidateHandPointers, long timestamp);

    /// <summary>
    /// Region where Kinect interaction happens.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Disposable interaction stream is disposed when sensor is set to null")]
    public class KinectRegion : ContentControl
    {
        public static readonly RoutedEvent HandPointerMoveEvent = EventManager.RegisterRoutedEvent(
            "HandPointerMove", RoutingStrategy.Bubble, typeof(EventHandler<HandPointerEventArgs>), typeof(KinectRegion));

        public static readonly RoutedEvent HandPointerEnterEvent = EventManager.RegisterRoutedEvent(
            "HandPointerEnter", RoutingStrategy.Direct, typeof(EventHandler<HandPointerEventArgs>), typeof(KinectRegion));

        public static readonly RoutedEvent HandPointerLeaveEvent = EventManager.RegisterRoutedEvent(
            "HandPointerLeave", RoutingStrategy.Direct, typeof(EventHandler<HandPointerEventArgs>), typeof(KinectRegion));

        public static readonly RoutedEvent HandPointerPressEvent = EventManager.RegisterRoutedEvent(
            "HandPointerPress", RoutingStrategy.Bubble, typeof(EventHandler<HandPointerEventArgs>), typeof(KinectRegion));

        public static readonly RoutedEvent HandPointerPressReleaseEvent = EventManager.RegisterRoutedEvent(
            "HandPointerPressRelease", RoutingStrategy.Bubble, typeof(EventHandler<HandPointerEventArgs>), typeof(KinectRegion));

        public static readonly RoutedEvent HandPointerGripEvent = EventManager.RegisterRoutedEvent(
            "HandPointerGrip", RoutingStrategy.Bubble, typeof(EventHandler<HandPointerEventArgs>), typeof(KinectRegion));

        public static readonly RoutedEvent HandPointerGripReleaseEvent = EventManager.RegisterRoutedEvent(
            "HandPointerGripRelease", RoutingStrategy.Bubble, typeof(EventHandler<HandPointerEventArgs>), typeof(KinectRegion));

        public static readonly RoutedEvent HandPointerGotCaptureEvent = EventManager.RegisterRoutedEvent(
            "HandPointerGotCapture", RoutingStrategy.Direct, typeof(EventHandler<HandPointerEventArgs>), typeof(KinectRegion));

        public static readonly RoutedEvent HandPointerLostCaptureEvent = EventManager.RegisterRoutedEvent(
            "HandPointerLostCapture", RoutingStrategy.Direct, typeof(EventHandler<HandPointerEventArgs>), typeof(KinectRegion));

        public static readonly RoutedEvent QueryInteractionStatusEvent = EventManager.RegisterRoutedEvent(
            "QueryInteractionStatus", RoutingStrategy.Bubble, typeof(EventHandler<QueryInteractionStatusEventArgs>), typeof(KinectRegion));

        public static readonly DependencyProperty KinectRegionProperty =
            DependencyProperty.RegisterAttached(
                "KinectRegion",
                typeof(KinectRegion),
                typeof(KinectRegion),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        public static readonly DependencyProperty IsPrimaryHandPointerOverProperty =
            DependencyProperty.RegisterAttached("IsPrimaryHandPointerOver", typeof(bool), typeof(KinectRegion), new UIPropertyMetadata(false));

        public static readonly DependencyProperty IsPressTargetProperty =
            DependencyProperty.RegisterAttached("IsPressTarget", typeof(bool), typeof(KinectRegion), new UIPropertyMetadata(false));

        public static readonly DependencyProperty IsGripTargetProperty =
            DependencyProperty.RegisterAttached("IsGripTarget", typeof(bool), typeof(KinectRegion), new UIPropertyMetadata(false));

        public static readonly DependencyProperty PressTargetPointProperty =
            DependencyProperty.RegisterAttached("PressTargetPoint", typeof(Point), typeof(KinectRegion), new UIPropertyMetadata(new Point(0.5, 0.5)));

        public static readonly DependencyProperty KinectSensorProperty = DependencyProperty.Register(
            "KinectSensor", typeof(KinectSensor), typeof(KinectRegion), new PropertyMetadata(null, (o, args) => ((KinectRegion)o).OnSensorChanged((KinectSensor)args.OldValue, (KinectSensor)args.NewValue)));

        public static readonly DependencyProperty IsCursorVisibleProperty = DependencyProperty.Register(
            "IsCursorVisible",
            typeof(bool),
            typeof(KinectRegion),
            new UIPropertyMetadata(true));

        public static readonly DependencyProperty QueryPrimaryUserTrackingIdCallbackProperty = DependencyProperty.Register(
            "QueryPrimaryUserTrackingIdCallback",
            typeof(QueryPrimaryUserTrackingIdCallback),
            typeof(KinectRegion),
            new UIPropertyMetadata(null));

        public static readonly DependencyProperty CursorPressingColorProperty = KinectCursorVisualizer.CursorPressingColorProperty.AddOwner(typeof(KinectRegion));

        public static readonly DependencyProperty CursorExtendedColor1Property = KinectCursorVisualizer.CursorExtendedColor1Property.AddOwner(typeof(KinectRegion));

        public static readonly DependencyProperty CursorExtendedColor2Property = KinectCursorVisualizer.CursorExtendedColor2Property.AddOwner(typeof(KinectRegion));

        public static readonly DependencyProperty CursorGrippedColor1Property = KinectCursorVisualizer.CursorGrippedColor1Property.AddOwner(typeof(KinectRegion));

        public static readonly DependencyProperty CursorGrippedColor2Property = KinectCursorVisualizer.CursorGrippedColor2Property.AddOwner(typeof(KinectRegion));

        public static readonly DependencyProperty PrimaryUserTrackingIdProperty;

        private static readonly DependencyPropertyKey PrimaryUserTrackingIdPropertyKey = DependencyProperty.RegisterReadOnly(
            "PrimaryUserTrackingId", 
            typeof(int), 
            typeof(KinectRegion), 
            new PropertyMetadata(
                KinectPrimaryUserTracker.InvalidUserTrackingId,
                (o, args) => ((KinectRegion)o).OnPrimaryUserTrackingIdChanged((int)args.OldValue, (int)args.NewValue)));

        /// <summary>
        /// Entry point for interaction stream functionality.
        /// </summary>
        private InteractionStream interactionStream;

        /// <summary>
        /// Component that takes the output of the
        /// core controller and sends it to WPF UI
        /// </summary>
        private KinectAdapter kinectAdapter;

        /// <summary>
        /// Intermediate storage for the skeleton data received from the Kinect sensor.
        /// </summary>
        private Skeleton[] skeletons;

        /// <summary>
        /// Intermediate storage for the user information received from interaction stream.
        /// </summary>
        private UserInfo[] userInfos;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "We need to OverrideMetadata in the static constructor")]
        static KinectRegion()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(KinectRegion), new FrameworkPropertyMetadata(typeof(KinectRegion)));

            PrimaryUserTrackingIdProperty = PrimaryUserTrackingIdPropertyKey.DependencyProperty;
        }

        public KinectRegion()
        {
            // check for design mode
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                this.InitializeKinectRegion();
            }
        }

        /// <summary>
        /// Event that is signaled when the primary user changes
        /// </summary>
        public event EventHandler<UserTrackingIdChangedEventArgs> PrimaryUserTrackingIdChanged;

        public event EventHandler<EventArgs> HandPointersUpdated
        {
            add
            {
                this.VerifyAccess();
                this.kinectAdapter.InternalHandPointersUpdated += value;
            }

            remove
            {
                this.VerifyAccess();
                this.kinectAdapter.InternalHandPointersUpdated -= value;
            }
        }

        public ReadOnlyObservableCollection<HandPointer> HandPointers
        {
            get
            {
                return this.kinectAdapter.ReadOnlyPublicHandPointers;
            }
        }

        public KinectSensor KinectSensor
        {
            get
            {
                this.VerifyAccess();
                return (KinectSensor)this.GetValue(KinectSensorProperty);
            }

            set
            {
                this.VerifyAccess();
                this.SetValue(KinectSensorProperty, value);
            }
        }

        public bool IsCursorVisible
        {
            get
            {
                this.VerifyAccess();
                return (bool)this.GetValue(IsCursorVisibleProperty);
            }

            set
            {
                this.VerifyAccess();
                this.SetValue(IsCursorVisibleProperty, value);
            }
        }

        public QueryPrimaryUserTrackingIdCallback QueryPrimaryUserTrackingIdCallback
        {
            get
            {
                this.VerifyAccess();
                return (QueryPrimaryUserTrackingIdCallback)this.GetValue(QueryPrimaryUserTrackingIdCallbackProperty);
            }

            set
            {
                this.VerifyAccess();
                this.SetValue(QueryPrimaryUserTrackingIdCallbackProperty, value);
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

        public int PrimaryUserTrackingId
        {
            get
            {
                this.VerifyAccess();
                return (int)this.GetValue(PrimaryUserTrackingIdProperty);
            }

            internal set
            {
                this.VerifyAccess();
                this.SetValue(PrimaryUserTrackingIdPropertyKey, value);
            }
        }

        internal UserInfo[] UserInfos
        {
            get
            {
                return this.userInfos;
            }
        }

        public static KinectRegion GetKinectRegion(DependencyObject obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            return (KinectRegion)obj.GetValue(KinectRegionProperty);
        }

        public static void SetKinectRegion(DependencyObject obj, KinectRegion value)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            if (value == null)
            {
                obj.ClearValue(KinectRegionProperty);
            }
            else
            {
                obj.SetValue(KinectRegionProperty, value);
            }
        }

        public static bool GetIsPrimaryHandPointerOver(DependencyObject obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            return (bool)obj.GetValue(IsPrimaryHandPointerOverProperty);
        }

        public static void SetIsPrimaryHandPointerOver(DependencyObject obj, bool value)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            if (!value)
            {
                obj.ClearValue(IsPrimaryHandPointerOverProperty);
            }
            else
            {
                obj.SetValue(IsPrimaryHandPointerOverProperty, true);
            }
        }

        public static bool GetIsPressTarget(DependencyObject obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            return (bool)obj.GetValue(IsPressTargetProperty);
        }

        public static void SetIsPressTarget(DependencyObject obj, bool value)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            obj.SetValue(IsPressTargetProperty, value);
        }

        public static bool GetIsGripTarget(DependencyObject obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            return (bool)obj.GetValue(IsGripTargetProperty);
        }

        public static void SetIsGripTarget(DependencyObject obj, bool value)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            obj.SetValue(IsGripTargetProperty, value);
        }

        public static Point GetPressTargetPoint(DependencyObject obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            return (Point)obj.GetValue(PressTargetPointProperty);
        }

        public static void SetPressTargetPoint(DependencyObject obj, Point value)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            obj.SetValue(PressTargetPointProperty, value);
        }

        /// <summary>
        /// Adds a handler for the HandPointerMove attached event
        /// </summary>
        /// <param name="element">UIElement that listens to this event</param>
        /// <param name="handler">Event Handler to be added</param>
        public static void AddHandPointerMoveHandler(UIElement element, EventHandler<HandPointerEventArgs> handler)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.AddHandler(KinectRegion.HandPointerMoveEvent, handler);
        }

        /// <summary>
        /// Removes a handler for the HandPointerMove attached event
        /// </summary>
        /// <param name="element">UIElement that listens to this event</param>
        /// <param name="handler">Event Handler to be removed</param>
        public static void RemoveHandPointerMoveHandler(UIElement element, EventHandler<HandPointerEventArgs> handler)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.RemoveHandler(KinectRegion.HandPointerMoveEvent, handler);
        }

        /// <summary>
        /// Adds a handler for the HandPointerEnter attached event
        /// </summary>
        /// <param name="element">UIElement that listens to this event</param>
        /// <param name="handler">Event Handler to be added</param>
        public static void AddHandPointerEnterHandler(UIElement element, EventHandler<HandPointerEventArgs> handler)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.AddHandler(KinectRegion.HandPointerEnterEvent, handler);
        }

        /// <summary>
        /// Removes a handler for the HandPointerEnter attached event
        /// </summary>
        /// <param name="element">UIElement that listens to this event</param>
        /// <param name="handler">Event Handler to be removed</param>
        public static void RemoveHandPointerEnterHandler(UIElement element, EventHandler<HandPointerEventArgs> handler)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.RemoveHandler(KinectRegion.HandPointerEnterEvent, handler);
        }

        /// <summary>
        /// Adds a handler for the HandPointerLeave attached event
        /// </summary>
        /// <param name="element">UIElement that listens to this event</param>
        /// <param name="handler">Event Handler to be added</param>
        public static void AddHandPointerLeaveHandler(UIElement element, EventHandler<HandPointerEventArgs> handler)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.AddHandler(KinectRegion.HandPointerLeaveEvent, handler);
        }

        /// <summary>
        /// Removes a handler for the HandPointerLeave attached event
        /// </summary>
        /// <param name="element">UIElement that listens to this event</param>
        /// <param name="handler">Event Handler to be removed</param>
        public static void RemoveHandPointerLeaveHandler(UIElement element, EventHandler<HandPointerEventArgs> handler)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.RemoveHandler(KinectRegion.HandPointerLeaveEvent, handler);
        }

        /// <summary>
        /// Adds a handler for the HandPointerPress attached event
        /// </summary>
        /// <param name="element">UIElement that listens to this event</param>
        /// <param name="handler">Event Handler to be added</param>
        public static void AddHandPointerPressHandler(UIElement element, EventHandler<HandPointerEventArgs> handler)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.AddHandler(KinectRegion.HandPointerPressEvent, handler);
        }

        /// <summary>
        /// Removes a handler for the HandPointerPress attached event
        /// </summary>
        /// <param name="element">UIElement that listens to this event</param>
        /// <param name="handler">Event Handler to be removed</param>
        public static void RemoveHandPointerPressHandler(UIElement element, EventHandler<HandPointerEventArgs> handler)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.RemoveHandler(KinectRegion.HandPointerPressEvent, handler);
        }

        /// <summary>
        /// Adds a handler for the HandPointerPressRelease attached event
        /// </summary>
        /// <param name="element">UIElement that listens to this event</param>
        /// <param name="handler">Event Handler to be added</param>
        public static void AddHandPointerPressReleaseHandler(UIElement element, EventHandler<HandPointerEventArgs> handler)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.AddHandler(KinectRegion.HandPointerPressReleaseEvent, handler);
        }

        /// <summary>
        /// Removes a handler for the HandPointerPressRelease attached event
        /// </summary>
        /// <param name="element">UIElement that listens to this event</param>
        /// <param name="handler">Event Handler to be removed</param>
        public static void RemoveHandPointerPressReleaseHandler(UIElement element, EventHandler<HandPointerEventArgs> handler)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.RemoveHandler(KinectRegion.HandPointerPressReleaseEvent, handler);
        }

        /// <summary>
        /// Adds a handler for the HandPointerGrip attached event
        /// </summary>
        /// <param name="element">UIElement that listens to this event</param>
        /// <param name="handler">Event Handler to be added</param>
        public static void AddHandPointerGripHandler(UIElement element, EventHandler<HandPointerEventArgs> handler)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.AddHandler(KinectRegion.HandPointerGripEvent, handler);
        }

        /// <summary>
        /// Removes a handler for the HandPointerGrip attached event
        /// </summary>
        /// <param name="element">UIElement that listens to this event</param>
        /// <param name="handler">Event Handler to be removed</param>
        public static void RemoveHandPointerGripHandler(UIElement element, EventHandler<HandPointerEventArgs> handler)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.RemoveHandler(KinectRegion.HandPointerGripEvent, handler);
        }

        /// <summary>
        /// Adds a handler for the HandPointerGripRelease attached event
        /// </summary>
        /// <param name="element">UIElement that listens to this event</param>
        /// <param name="handler">Event Handler to be added</param>
        public static void AddHandPointerGripReleaseHandler(UIElement element, EventHandler<HandPointerEventArgs> handler)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.AddHandler(KinectRegion.HandPointerGripReleaseEvent, handler);
        }

        /// <summary>
        /// Removes a handler for the HandPointerGripRelease attached event
        /// </summary>
        /// <param name="element">UIElement that listens to this event</param>
        /// <param name="handler">Event Handler to be removed</param>
        public static void RemoveHandPointerGripReleaseHandler(UIElement element, EventHandler<HandPointerEventArgs> handler)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.RemoveHandler(KinectRegion.HandPointerGripReleaseEvent, handler);
        }

        /// <summary>
        /// Adds a handler for the HandPointerGotCapture attached event
        /// </summary>
        /// <param name="element">UIElement that listens to this event</param>
        /// <param name="handler">Event Handler to be added</param>
        public static void AddHandPointerGotCaptureHandler(UIElement element, EventHandler<HandPointerEventArgs> handler)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.AddHandler(KinectRegion.HandPointerGotCaptureEvent, handler);
        }

        /// <summary>
        /// Removes a handler for the HandPointerGotCapture attached event
        /// </summary>
        /// <param name="element">UIElement that listens to this event</param>
        /// <param name="handler">Event Handler to be removed</param>
        public static void RemoveHandPointerGotCaptureHandler(UIElement element, EventHandler<HandPointerEventArgs> handler)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.RemoveHandler(KinectRegion.HandPointerGotCaptureEvent, handler);
        }

        /// <summary>
        /// Adds a handler for the HandPointerLostCapture attached event
        /// </summary>
        /// <param name="element">UIElement that listens to this event</param>
        /// <param name="handler">Event Handler to be added</param>
        public static void AddHandPointerLostCaptureHandler(UIElement element, EventHandler<HandPointerEventArgs> handler)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.AddHandler(KinectRegion.HandPointerLostCaptureEvent, handler);
        }

        /// <summary>
        /// Removes a handler for the HandPointerLostCapture attached event
        /// </summary>
        /// <param name="element">UIElement that listens to this event</param>
        /// <param name="handler">Event Handler to be removed</param>
        public static void RemoveHandPointerLostCaptureHandler(UIElement element, EventHandler<HandPointerEventArgs> handler)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.RemoveHandler(KinectRegion.HandPointerLostCaptureEvent, handler);
        }

        /// <summary>
        /// Adds a handler for the QueryGripInteractionStatus attached event
        /// </summary>
        /// <param name="element">UIElement that listens to this event</param>
        /// <param name="handler">Event Handler to be added</param>
        public static void AddQueryInteractionStatusHandler(UIElement element, EventHandler<QueryInteractionStatusEventArgs> handler)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.AddHandler(KinectRegion.QueryInteractionStatusEvent, handler);
        }

        /// <summary>
        /// Removes a handler for the QueryGripInteractionStatus attached event
        /// </summary>
        /// <param name="element">UIElement that listens to this event</param>
        /// <param name="handler">Event Handler to be removed</param>
        public static void RemoveQueryInteractionStatusHandler(UIElement element, EventHandler<QueryInteractionStatusEventArgs> handler)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.RemoveHandler(KinectRegion.QueryInteractionStatusEvent, handler);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.CheckForNested();
        }

        private void InitializeKinectRegion()
        {
            // KinectRegion is an invisible control, so no use tabbing to it.
            this.IsTabStop = false;

            this.kinectAdapter = new KinectAdapter(this);
            SetKinectRegion(this, this);
        }

        private void CheckForNested()
        {
            // Check for nested KinectRegions
            DependencyObject search = VisualTreeHelper.GetParent(this);
            while (search != null)
            {
                var region = search as KinectRegion;
                if (region != null)
                {
                    throw new InvalidOperationException("Nested Kinect region elements are not allowed.");
                }

                // move next
                search = VisualTreeHelper.GetParent(search);
            }
        }
        
        /// <summary>
        /// Invokes the PrimaryUserTrackingIdChanged event to signal that the primary user has changed
        /// </summary>
        /// <param name="oldTrackingId">Tracking identifier of the old primary user</param>
        /// <param name="newTrackingId">Tracking identifier of the new primary user</param>
        private void OnPrimaryUserTrackingIdChanged(int oldTrackingId, int newTrackingId)
        {
            EventHandler<UserTrackingIdChangedEventArgs> handler = this.PrimaryUserTrackingIdChanged;
            if (null != handler)
            {
                handler(this, new UserTrackingIdChangedEventArgs(oldTrackingId, newTrackingId));
            }
        }

        private void OnSensorChanged(KinectSensor oldSensor, KinectSensor newSensor)
        {
            this.CheckForNested();

            if (this.kinectAdapter != null)
            {
                // Clear hand pointer state since hand pointers corresponding to one
                // sensor can't be reused when processing data from another sensor.
                this.kinectAdapter.ClearHandPointers();
            }

            if (oldSensor != null)
            {
                oldSensor.DepthFrameReady -= this.SensorDepthFrameReady;
                oldSensor.SkeletonFrameReady -= this.SensorSkeletonFrameReady;

                this.skeletons = null;
                this.userInfos = null;

                this.interactionStream.InteractionFrameReady -= this.InteractionFrameReady;
                this.interactionStream.Dispose();
                this.interactionStream = null;
            }

            if (newSensor != null)
            {
                this.interactionStream = new InteractionStream(newSensor, this.kinectAdapter);
                this.interactionStream.InteractionFrameReady += this.InteractionFrameReady;

                // Allocate space to put the skeleton and interaction data we'll receive
                this.skeletons = new Skeleton[newSensor.SkeletonStream.FrameSkeletonArrayLength];
                this.userInfos = new UserInfo[InteractionFrame.UserInfoArrayLength];

                newSensor.DepthFrameReady += this.SensorDepthFrameReady;
                newSensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;
            }
        }

        /// <summary>
        /// Handler for the Kinect sensor's DepthFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="depthImageFrameReadyEventArgs">event arguments</param>
        private void SensorDepthFrameReady(object sender, DepthImageFrameReadyEventArgs depthImageFrameReadyEventArgs)
        {
            // Even though we un-register all our event handlers when the sensor
            // changes, there may still be an event for the old sensor in the queue
            // due to the way the KinectSensor delivers events.  So check again here.
            if (this.KinectSensor != sender)
            {
                return;
            }

            using (DepthImageFrame depthFrame = depthImageFrameReadyEventArgs.OpenDepthImageFrame())
            {
                if (null != depthFrame)
                {
                    try
                    {
                        // Hand data to Interaction framework to be processed
                        this.interactionStream.ProcessDepth(depthFrame.GetRawPixelData(), depthFrame.Timestamp);
                    }
                    catch (InvalidOperationException)
                    {
                        // DepthFrame functions may throw when the sensor gets
                        // into a bad state.  Ignore the frame in that case.
                    }
                }
            }
        }

        /// <summary>
        /// Handler for the Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="skeletonFrameReadyEventArgs">event arguments</param>
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs skeletonFrameReadyEventArgs)
        {
            // Even though we un-register all our event handlers when the sensor
            // changes, there may still be an event for the old sensor in the queue
            // due to the way the KinectSensor delivers events.  So check again here.
            if (this.KinectSensor != sender)
            {
                return;
            }

            using (SkeletonFrame skeletonFrame = skeletonFrameReadyEventArgs.OpenSkeletonFrame())
            {
                if (null != skeletonFrame)
                {
                    try
                    {
                        // Copy the skeleton data from the frame to an array used for temporary storage
                        skeletonFrame.CopySkeletonDataTo(this.skeletons);

                        var accelerometerReading = this.KinectSensor.AccelerometerGetCurrentReading();

                        // Hand data to Interaction framework to be processed
                        this.interactionStream.ProcessSkeleton(this.skeletons, accelerometerReading, skeletonFrame.Timestamp);
                    }
                    catch (InvalidOperationException)
                    {
                        // SkeletonFrame functions may throw when the sensor gets
                        // into a bad state.  Ignore the frame in that case.
                    }
                }
            }
        }

        /// <summary>
        /// Event handler for InteractionStream's InteractionFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "KinectSensor", Justification = "KinectSensor is valid in a debug string"),
         SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "KinectRegion", Justification = "KinectRegion is valid in a debug string"),
         SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "BeginInvoke", Justification = "BeginInvoke is valid in a debug string"),
         SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Diagnostics.Debugger.Log(System.Int32,System.String,System.String)", Justification = "Debugger-only messages.  Will never be localized.")]
        private void InteractionFrameReady(object sender, InteractionFrameReadyEventArgs e)
        {
            // Check for a null userInfos since we may still get posted events
            // from the stream after we have unregistered our event handler and
            // deleted our buffers.
            if (this.userInfos == null)
            {
                return;
            }

            if (this.kinectAdapter.IsInInteractionFrame)
            {
                Debugger.Log(
                    0,
                    "warning",
                    "Application might have raised modal UI (e.g.: message box) while handling KinectRegion event. Applications are advised to raise modal UI via a call to System.Windows.Threading.Dispatcher.BeginInvoke instead.\n");
                return;
            }

            UserInfo[] localUserInfos = null;
            long timestamp = 0;

            using (InteractionFrame interactionFrame = e.OpenInteractionFrame())
            {
                if (interactionFrame != null)
                {
                    // Copy interaction frame data so we can dispose interaction frame
                    // right away, even if data processing/event handling takes a while.
                    interactionFrame.CopyInteractionDataTo(this.userInfos);
                    timestamp = interactionFrame.Timestamp;
                    localUserInfos = this.userInfos;
                }
            }

            if (localUserInfos != null)
            {
                this.kinectAdapter.BeginInteractionFrame();

                try
                {
                    bool wasProcessingAborted = false;

                    // Distribute routed events based on the state of all hand pointers
                    for (int userIndex = 0; userIndex < localUserInfos.Length; ++userIndex)
                    {
                        var user = localUserInfos[userIndex];
                        foreach (var handPointer in user.HandPointers)
                        {
                            this.HandleHandPointerData(timestamp, user, userIndex, handPointer);

                            if (localUserInfos != this.userInfos)
                            {
                                // Double-check that user info data being processed is still valid.
                                // Client might have invalidated it by changing the KinectSensor
                                // while handling a KinectRegion event.
                                wasProcessingAborted = true;
                                break;
                            }
                        }
                    }

                    if (wasProcessingAborted)
                    {
                        Debugger.Log(
                            0,
                            "warning",
                            "Application might have changed KinectSensor while handling KinectRegion event. Applications are advised to change KinectSensor via a call to System.Windows.Threading.Dispatcher.BeginInvoke instead.\n");
                    }
                    else
                    {
                        this.PrimaryUserTrackingId = this.kinectAdapter.ChoosePrimaryUser(timestamp, this.PrimaryUserTrackingId, this.QueryPrimaryUserTrackingIdCallback);
                    }
                }
                finally
                {
                    this.kinectAdapter.EndInteractionFrame();
                }
            }
        }

        private void HandleHandPointerData(long timeStamp, UserInfo userInfo, int userIndex, InteractionHandPointer handPointer)
        {
            var interactionData = new InteractionFrameData
            {
                TimeStampOfLastUpdate = timeStamp,
                TrackingId = userInfo.SkeletonTrackingId,
                PlayerIndex = userIndex,
                HandType = EnumHelper.ConvertHandType(handPointer.HandType),
                IsTracked = handPointer.IsTracked,
                IsActive = handPointer.IsActive,
                IsInteractive = handPointer.IsInteractive,
                IsPressed = handPointer.IsPressed,
                IsPrimaryHandOfUser = handPointer.IsPrimaryForUser,
                IsPrimaryUser = (userInfo.SkeletonTrackingId == this.PrimaryUserTrackingId) && (userInfo.SkeletonTrackingId != KinectPrimaryUserTracker.InvalidUserTrackingId),
                HandEventType = EnumHelper.ConvertHandEventType(handPointer.HandEventType),
                X = handPointer.X,
                Y = handPointer.Y,
                Z = handPointer.PressExtent
            };

            this.kinectAdapter.HandleHandPointerData(interactionData);
        }
    }
}
