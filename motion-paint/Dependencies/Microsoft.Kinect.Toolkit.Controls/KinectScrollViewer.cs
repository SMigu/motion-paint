// -----------------------------------------------------------------------
// <copyright file="KinectScrollViewer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.Controls
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Threading;

    using Microsoft.Kinect.Toolkit.Interaction;

    /// <summary>
    /// A ScrollViewer class that responds to Kinect events
    /// </summary>
    public sealed class KinectScrollViewer : ScrollViewer
    {
        public static readonly DependencyProperty ContentMarginProperty = DependencyProperty.Register(
            "ContentMargin", typeof(double), typeof(KinectScrollViewer), new PropertyMetadata(0.0));

        public static readonly DependencyProperty HoverBackgroundProperty = DependencyProperty.Register(
             "HoverBackground", typeof(Brush), typeof(KinectScrollViewer), new PropertyMetadata(null)); 

        private const double MinSpeedForInertial = 1800.0;

        private const double SecondsBeforeScrollbarHide = 2.0;

        private const double HorizontalScaleDownMax = 0.06;

        private const double VerticalScaleDownMax = 0.06;

        private const double BounceBackAnimationSeconds = 0.08;

        private const double HorizontalTranslationMax = 80;

        private const double VerticalTranslationMax = 80;

        private const int GripDetectorDelayMs = 100;

        private const int MinTimeSinceGripBeforeReleaseInertialScrollMs = 250;

        private const int HandPointerSamplesToGather = 50;

        private static readonly bool IsInDesignMode = DesignerProperties.GetIsInDesignMode(new DependencyObject());

        private static readonly DependencyProperty IsPrimaryHandPointerOverProperty = DependencyProperty.Register(
            "IsPrimaryHandPointerOver", typeof(bool), typeof(KinectScrollViewer), new PropertyMetadata(false, (o, args) => ((KinectScrollViewer)o).OnIsPrimaryHandPointerOverChanged((bool)args.NewValue)));

        private readonly Lazy<ExponentialEase> exponentialEase =
            new Lazy<ExponentialEase>(() => new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3.0 });

        private readonly ScrollViewerInertiaScroller scrollViewerInertiaScroller = new ScrollViewerInertiaScroller();

        private readonly DispatcherTimer scrollMoveTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(SecondsBeforeScrollbarHide) };

        private readonly HandPointerSampleTracker sampleTracker = new HandPointerSampleTracker(HandPointerSamplesToGather);

        private KinectRegionBinder kinectRegionBinder;

        private HandPointer capturedHandPointer;

        private Point gripPoint;

        private Point startingScrollOffset;

        private GripState lastGripState;

        private ScrollContentPresenter scrollContentPresenter;

        private TranslateTransform translateTransform;

        private ScaleTransform scaleTransform;

        private double horizontalTranslation;

        private double verticalTranslation;

        private double horizontalScale = 1.0;

        private double verticalScale = 1.0;

        private long lastTimeStamp;

        private long lastGripTimeStamp;

        private long lastGripReleaseTimeStamp;

        private Point lastMousePoint;

        private HandPointer grippedHandpointer;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "We need to OverrideMetadata in the static constructor")]
        static KinectScrollViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(KinectScrollViewer), new FrameworkPropertyMetadata(typeof(KinectScrollViewer)));
        }

        /// <summary>
        /// Initializes a new instance of the KinectScrollViewer class.
        /// </summary>
        public KinectScrollViewer()
        {
            this.SetScrollBarVisualState("Stopped");
            this.lastGripState = GripState.Released;
            this.ClipToBounds = true;

            if (!IsInDesignMode)
            {
                this.InitializeKinectScrollViewer();
            }
        }

        private enum GripState
        {
            Released,
            Gripped
        }

        public double ContentMargin
        {
            get
            {
                return (double)this.GetValue(ContentMarginProperty);
            }

            set
            {
                this.SetValue(ContentMarginProperty, value);
            }
        }

        public Brush HoverBackground
        {
            get
            {
                return (Brush)this.GetValue(HoverBackgroundProperty);
            }

            set
            {
                this.SetValue(HoverBackgroundProperty, value);
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.scrollContentPresenter = this.Template.FindName("PART_ScrollContentPresenter", this) as ScrollContentPresenter;
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            base.OnMouseMove(e);

            Point currentMousePosition = e.GetPosition(this);
            if (currentMousePosition != this.lastMousePoint)
            {
                e.Handled = true;
                this.scrollMoveTimer.Stop();
                this.SetScrollBarVisualState("MouseScrolling");
                this.scrollMoveTimer.Start();
            }

            this.lastMousePoint = currentMousePosition;
        }

        protected override void OnScrollChanged(ScrollChangedEventArgs e)
        {
            base.OnScrollChanged(e);
            this.scrollMoveTimer.Stop();

            if (null == this.capturedHandPointer && AnimationState.None == this.scrollViewerInertiaScroller.AnimationState)
            {
                // Scrolling other than grip
                this.SetScrollBarVisualState("MouseScrolling");
            }

            // apply any bounceback animation if we hit the scroll limits fast enough.
            if (this.scrollContentPresenter != null)
            {
                this.scrollContentPresenter.RenderTransform = Transform.Identity;

                // apply any bounceback animation if we hit the scroll limits fast enough.
                if (AnimationState.Stopping == this.scrollViewerInertiaScroller.AnimationState)
                {
                    if (Math.Abs(this.scrollViewerInertiaScroller.BounceBackVelocity.X) > MinSpeedForInertial
                        || Math.Abs(this.scrollViewerInertiaScroller.BounceBackVelocity.Y) > MinSpeedForInertial)
                    {
                        double horizontalBounceVelocity = this.scrollViewerInertiaScroller.BounceBackVelocity.X;
                        double verticalBounceVelocity = this.scrollViewerInertiaScroller.BounceBackVelocity.Y;
                        this.scrollViewerInertiaScroller.BounceBackVelocity = new Vector(0.0, 0.0);
                        double horizontalBounceTranslation = Math.Sign(horizontalBounceVelocity) * -HorizontalTranslationMax;
                        double verticalBounceTranslation = Math.Sign(verticalBounceVelocity) * -VerticalTranslationMax;
                        double horizontalScaleOrigin = Math.Sign(horizontalBounceVelocity) < 0 ? this.ActualWidth : 0;
                        double verticalScaleOrigin = Math.Sign(verticalBounceVelocity) < 0 ? this.ActualHeight : 0;

                        this.translateTransform = new TranslateTransform(0, 0);
                        this.scaleTransform = new ScaleTransform(1.0, 1.0, horizontalScaleOrigin, verticalScaleOrigin);
                        var transformGroup = new TransformGroup();
                        transformGroup.Children.Add(this.translateTransform);
                        transformGroup.Children.Add(this.scaleTransform);
                        this.scrollContentPresenter.RenderTransform = transformGroup;

                        // horizontal bounce animations
                        if (Math.Abs(horizontalBounceVelocity) > MinSpeedForInertial)
                        {
                            var horizontalTranslateAnimation = new DoubleAnimation(
                                0, horizontalBounceTranslation, TimeSpan.FromSeconds(BounceBackAnimationSeconds))
                                { AutoReverse = true, EasingFunction = this.exponentialEase.Value };
                            var horizontalScaleAnimation = new DoubleAnimation(
                                1.0, 1.0 - HorizontalScaleDownMax, TimeSpan.FromSeconds(BounceBackAnimationSeconds))
                                { AutoReverse = true, EasingFunction = this.exponentialEase.Value };
                            this.translateTransform.BeginAnimation(TranslateTransform.XProperty, horizontalTranslateAnimation);
                            this.scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, horizontalScaleAnimation);
                        }

                        // vertical bounce animations
                        if (Math.Abs(verticalBounceVelocity) > MinSpeedForInertial)
                        {
                            var verticalTranslateAnimation = new DoubleAnimation(
                                0, verticalBounceTranslation, TimeSpan.FromSeconds(BounceBackAnimationSeconds))
                                { AutoReverse = true, EasingFunction = this.exponentialEase.Value };
                            var verticalScaleAnimation = new DoubleAnimation(
                                1.0, 1.0 - VerticalScaleDownMax, TimeSpan.FromSeconds(BounceBackAnimationSeconds))
                                { AutoReverse = true, EasingFunction = this.exponentialEase.Value };
                            this.translateTransform.BeginAnimation(TranslateTransform.YProperty, verticalTranslateAnimation);
                            this.scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, verticalScaleAnimation);
                        }
                    }
                }
            }

            this.scrollMoveTimer.Start();
        }

        private void InitializeKinectScrollViewer()
        {
            KinectRegion.AddHandPointerGotCaptureHandler(this, this.OnHandPointerCaptured);
            KinectRegion.AddHandPointerLostCaptureHandler(this, this.OnHandPointerLostCapture);
            KinectRegion.AddHandPointerEnterHandler(this, this.OnHandPointerEnter);
            KinectRegion.AddHandPointerMoveHandler(this, this.OnHandPointerMove);
            KinectRegion.AddHandPointerPressHandler(this, this.OnHandPointerPress);
            KinectRegion.AddHandPointerGripHandler(this, this.OnHandPointerGrip);
            KinectRegion.AddHandPointerGripReleaseHandler(this, this.OnHandPointerGripRelease);
            KinectRegion.AddQueryInteractionStatusHandler(this, this.OnQueryInteractionStatus);
            KinectRegion.SetIsGripTarget(this, true);
            this.scrollMoveTimer.Tick += this.OnScrollMoveTimerTick;
            this.scrollViewerInertiaScroller.SlowEnoughForSelectionChanged += this.OnSlowEnoughForSelectionChanged;

            // Create KinectRegion binding
            this.kinectRegionBinder = new KinectRegionBinder(this);
            this.kinectRegionBinder.OnKinectRegionChanged += this.OnKinectRegionChanged;    
        }

        private void OnKinectRegionChanged(object sender, KinectRegion oldKinectRegion, KinectRegion newKinectRegion)
        {
            if (oldKinectRegion != null)
            {
                oldKinectRegion.HandPointersUpdated -= this.KinectScrollViewerHandPointersUpdated;
            }

            if (newKinectRegion != null)
            {
                newKinectRegion.HandPointersUpdated += this.KinectScrollViewerHandPointersUpdated;

                // Bind our IsPrimaryHandpointerOver dependency property
                var binding = new Binding { Source = this, Path = new PropertyPath(KinectRegion.IsPrimaryHandPointerOverProperty) };
                BindingOperations.SetBinding(this, IsPrimaryHandPointerOverProperty, binding);
            }
        }

        private void KinectScrollViewerHandPointersUpdated(object sender, EventArgs e)
        {
            var kinectRegion = (KinectRegion)sender;
            var primaryHandPointer = kinectRegion.HandPointers.FirstOrDefault(hp => hp.IsPrimaryHandOfPrimaryUser);
            if (primaryHandPointer == null)
            {
                return;
            }

            if (primaryHandPointer.HandEventType == HandEventType.Grip)
            {
                if (this.capturedHandPointer == primaryHandPointer)
                {
                    return;
                }

                this.grippedHandpointer = primaryHandPointer;
                return;
            }

            if (this.grippedHandpointer == primaryHandPointer && primaryHandPointer.HandEventType == HandEventType.GripRelease)
            {
                this.grippedHandpointer = null;
            }
        }

        private void OnIsPrimaryHandPointerOverChanged(bool isOver)
        {
            VisualStateManager.GoToState(this, isOver ? "HandPointerOver" : "Normal", true);
        }

        private bool HasHorizontalTransform()
        {
            return Math.Abs(this.horizontalTranslation) > double.Epsilon || Math.Abs(1.0 - this.horizontalScale) > double.Epsilon;
        }

        private bool HasVerticalTransform()
        {
            return Math.Abs(this.verticalTranslation) > double.Epsilon || Math.Abs(1.0 - this.verticalScale) > double.Epsilon;
        }

        private bool HasTransform()
        {
            return this.HasHorizontalTransform() || this.HasVerticalTransform();
        }

        private void OnScrollMoveTimerTick(object sender, EventArgs args)
        {
            this.scrollMoveTimer.Stop();
            this.SetScrollBarVisualState("Stopped");
        }
        
        private void OnHandPointerEnter(object sender, HandPointerEventArgs kinectHandPointerEventArgs)
        {
            if (kinectHandPointerEventArgs.HandPointer.IsPrimaryHandOfPrimaryUser)
            {
                kinectHandPointerEventArgs.Handled = true;
                if (this.grippedHandpointer == kinectHandPointerEventArgs.HandPointer)
                {
                    this.HandleHandPointerGrip(kinectHandPointerEventArgs.HandPointer);
                    this.grippedHandpointer = null;
                }
            }
        }

        private void OnHandPointerMove(object sender, HandPointerEventArgs kinectHandPointerEventArgs)
        {
            if (this.Equals(kinectHandPointerEventArgs.HandPointer.Captured))
            {
                kinectHandPointerEventArgs.Handled = true;

                var currentPosition = kinectHandPointerEventArgs.HandPointer.GetPosition(this);

                this.sampleTracker.AddSample(currentPosition.X, currentPosition.Y, kinectHandPointerEventArgs.HandPointer.TimestampOfLastUpdate);
                this.lastTimeStamp = kinectHandPointerEventArgs.HandPointer.TimestampOfLastUpdate;
                if (this.lastGripState == GripState.Released)
                {
                    return;
                }

                if (this.scrollViewerInertiaScroller.AnimationState == AnimationState.Inertial)
                {
                    this.scrollViewerInertiaScroller.Stop();
                }

                if (!kinectHandPointerEventArgs.HandPointer.IsInteractive)
                {
                    if (this.AttemptInertialScroll())
                    {
                        this.lastGripState = GripState.Released;
                        return;                       
                    }
                }
                
                var diffVector = this.gripPoint - currentPosition;

                var horizontalScaleOrigin = 0.0;
                var verticalScaleOrigin = 0.0;
                this.horizontalScale = 1.0;
                this.verticalScale = 1.0;
                this.horizontalTranslation = 0.0;
                this.verticalTranslation = 0.0;
                const double EaseFactor = 1.0 / HorizontalTranslationMax;

                if (this.HorizontalScrollBarVisibility != ScrollBarVisibility.Disabled)
                {
                    var scrollOffset = diffVector.X + this.startingScrollOffset.X;
                    this.ScrollToHorizontalOffset(scrollOffset);

                    if (scrollOffset < 0)
                    {
                        var easedOutTranslation = Math.Tanh(-scrollOffset * EaseFactor) * HorizontalTranslationMax;
                        horizontalScaleOrigin = this.ActualWidth;
                        this.horizontalTranslation = easedOutTranslation;
                    }
                    else if (scrollOffset > this.ScrollableWidth)
                    {
                        var easedOutTranslation = Math.Tanh((this.ScrollableWidth - scrollOffset) * EaseFactor) * HorizontalTranslationMax;
                        this.horizontalTranslation = easedOutTranslation;
                    }

                    if (Math.Abs(this.horizontalTranslation) > double.Epsilon)
                    {
                        this.horizontalScale = 1.0 - (HorizontalScaleDownMax * (Math.Abs(this.horizontalTranslation) / HorizontalTranslationMax));
                    }
                }

                if (this.VerticalScrollBarVisibility != ScrollBarVisibility.Disabled)
                {
                    var scrollOffset = diffVector.Y + this.startingScrollOffset.Y;
                    this.ScrollToVerticalOffset(scrollOffset);
                    if (scrollOffset < 0)
                    {
                        this.verticalTranslation = Math.Min(-scrollOffset, VerticalTranslationMax);
                        verticalScaleOrigin = this.ActualHeight;
                    }
                    else if (scrollOffset > this.ScrollableHeight)
                    {
                        this.verticalTranslation = Math.Max(this.ScrollableHeight - scrollOffset, -VerticalTranslationMax);
                    }

                    if (Math.Abs(this.verticalTranslation) > double.Epsilon)
                    {
                        this.verticalScale = 1.0 - (VerticalScaleDownMax * (Math.Abs(this.verticalTranslation) / VerticalTranslationMax));
                    }
                }

                if (this.scrollContentPresenter != null)
                {
                    if (this.HasTransform())
                    {
                        var transformGroup = new TransformGroup();
                        this.translateTransform = new TranslateTransform(this.horizontalTranslation, this.verticalTranslation);
                        this.scaleTransform = new ScaleTransform(
                            this.horizontalScale, this.verticalScale, horizontalScaleOrigin, verticalScaleOrigin);
                        transformGroup.Children.Add(this.translateTransform);
                        transformGroup.Children.Add(this.scaleTransform);
                        this.scrollContentPresenter.RenderTransform = transformGroup;
                    }
                    else
                    {
                        this.scrollContentPresenter.RenderTransform = Transform.Identity;
                    }
                }
            }
        }

        private void OnHandPointerPress(object sender, HandPointerEventArgs kinectHandPointerEventArgs)
        {
            if (this.Equals(kinectHandPointerEventArgs.HandPointer.Captured))
            {
                kinectHandPointerEventArgs.Handled = true;

                if (this.scrollViewerInertiaScroller.AnimationState == AnimationState.Inertial && kinectHandPointerEventArgs.HandPointer.IsInteractive)
                {
                    // release capture on a press event if we're in the middle of inertial scrolling
                    kinectHandPointerEventArgs.HandPointer.Capture(null);
                    this.lastGripState = GripState.Released;
                    this.scrollViewerInertiaScroller.Stop();
                }
            }
        }

        private void OnHandPointerGrip(object sender, HandPointerEventArgs kinectHandPointerEventArgs)
        {
            if (kinectHandPointerEventArgs.HandPointer.IsPrimaryUser && kinectHandPointerEventArgs.HandPointer.IsPrimaryHandOfUser && kinectHandPointerEventArgs.HandPointer.IsInteractive)
            {
                this.HandleHandPointerGrip(kinectHandPointerEventArgs.HandPointer);
                kinectHandPointerEventArgs.Handled = true;
            }
        }

        private void HandleHandPointerGrip(HandPointer handPointer)
        {
            if (handPointer == null)
            {
                return;
            }

            if (this.capturedHandPointer != handPointer)
            {
                if (handPointer.Captured == null)
                {
                    // Only capture hand pointer if it isn't already captured
                    handPointer.Capture(this);
                }
                else
                {
                    // Some other control has capture, ignore grip
                    return;
                }
            }

            this.lastGripState = GripState.Gripped;
            this.lastGripTimeStamp = handPointer.TimestampOfLastUpdate;
            this.scrollViewerInertiaScroller.Stop();
            this.gripPoint = handPointer.GetPosition(this);
            this.startingScrollOffset = new Point(this.HorizontalOffset, this.VerticalOffset);
        }

        private void DoTransformAnimations()
        {
            if (this.scrollContentPresenter != null)
            {
                if (this.HasHorizontalTransform())
                {
                    var scaleAnimation = new DoubleAnimation(
                        this.horizontalScale, 1.0, TimeSpan.FromSeconds(BounceBackAnimationSeconds))
                                                     {
                                                         EasingFunction =
                                                             this.exponentialEase.Value
                                                     };
                    var translateAnimation = new DoubleAnimation(
                        this.horizontalTranslation, 0, TimeSpan.FromSeconds(BounceBackAnimationSeconds))
                                                         {
                                                             EasingFunction =
                                                                 this.exponentialEase.Value
                                                         };
                    this.scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
                    this.translateTransform.BeginAnimation(TranslateTransform.XProperty, translateAnimation);
                }

                if (this.HasVerticalTransform())
                {
                    var scaleAnimation = new DoubleAnimation(
                        this.verticalScale, 1.0, TimeSpan.FromSeconds(BounceBackAnimationSeconds))
                                                     {
                                                         EasingFunction =
                                                             this.exponentialEase.Value
                                                     };
                    var translateAnimation = new DoubleAnimation(
                        this.verticalTranslation, 0, TimeSpan.FromSeconds(BounceBackAnimationSeconds))
                                                         {
                                                             EasingFunction =
                                                                 this.exponentialEase.Value
                                                         };
                    this.scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
                    this.translateTransform.BeginAnimation(TranslateTransform.YProperty, translateAnimation);
                }
            }
        }

        private void OnHandPointerGripRelease(object sender, HandPointerEventArgs kinectHandPointerEventArgs)
        {
            this.lastGripReleaseTimeStamp = kinectHandPointerEventArgs.HandPointer.TimestampOfLastUpdate;

            if (this.Equals(kinectHandPointerEventArgs.HandPointer.Captured))
            {
                kinectHandPointerEventArgs.Handled = true;
                this.lastGripState = GripState.Released;
                this.DoTransformAnimations();

                if (this.scrollViewerInertiaScroller.AnimationState != AnimationState.Inertial)
                {
                    if (this.AttemptInertialScroll())
                    {
                        return;
                    }
                }

                if (this.scrollViewerInertiaScroller.SlowEnoughForSelection)
                {
                    kinectHandPointerEventArgs.HandPointer.Capture(null);
                }
            }
        }

        private void OnQueryInteractionStatus(object sender, QueryInteractionStatusEventArgs queryInteractionStatusEventArgs)
        {
            if (this.Equals(queryInteractionStatusEventArgs.HandPointer.Captured))
            {
                queryInteractionStatusEventArgs.IsInGripInteraction = this.lastGripState == GripState.Gripped;
                queryInteractionStatusEventArgs.Handled = true;
            }
        }

        private void OnHandPointerCaptured(object sender, HandPointerEventArgs kinectHandPointerEventArgs)
        {
            if (this.capturedHandPointer != null)
            {
                // Release capture on any previous captured handpointer
                this.capturedHandPointer.Capture(null);
            }

            this.capturedHandPointer = kinectHandPointerEventArgs.HandPointer;
            kinectHandPointerEventArgs.Handled = true;
            this.SetScrollBarVisualState("GripScrolling");
            this.scrollMoveTimer.Stop();
        }

        private void OnHandPointerLostCapture(object sender, HandPointerEventArgs kinectHandPointerEventArgs)
        {
            if (this.capturedHandPointer == kinectHandPointerEventArgs.HandPointer)
            {
                this.capturedHandPointer = null;
                this.lastGripState = GripState.Released;
                this.scrollMoveTimer.Start();
                kinectHandPointerEventArgs.Handled = true;
                this.DoTransformAnimations();
            }
        }

        private void OnSlowEnoughForSelectionChanged(object sender, EventArgs e)
        {
            // We keep capture of the hand pointer during inertial scrolling so we can respond to a push-to-stop.  
            // If we're no longer scrolling and we've had a grip release, release the capture of the hand pointer.
            if (this.capturedHandPointer != null && this.scrollViewerInertiaScroller.SlowEnoughForSelection && this.lastGripState == GripState.Released)
            {
                this.capturedHandPointer.Capture(null);
            }
        }

        private void SetScrollBarVisualState(string visualState)
        {
            var horizontalScrollBar = this.Template.FindName("PART_HorizontalScrollBar", this) as ScrollBar;
            var verticalScrollBar = this.Template.FindName("PART_VerticalScrollBar", this) as ScrollBar;

            if (null != horizontalScrollBar && ScrollBarVisibility.Disabled != this.HorizontalScrollBarVisibility)
            {
                VisualStateManager.GoToState(horizontalScrollBar, visualState, true);
            }

            if (null != verticalScrollBar && ScrollBarVisibility.Disabled != this.VerticalScrollBarVisibility)
            {
                VisualStateManager.GoToState(verticalScrollBar, visualState, true);
            }
        }

        /// <summary>
        /// Convert hand pointer velocity into a velocity ready to be applied to scroll viewer
        /// for an inertial scroll action.
        /// </summary>
        /// <param name="handPointerVelocity">
        /// Hand pointer velocity to be converted to inertial scroll velocity.
        /// </param>
        /// <param name="doHorizontalInertialScroll">
        /// [out] Upon return, is true if a horizontal inertial scroll is recommended.
        /// false otherwise
        /// </param>
        /// <param name="doVerticalInertialScroll">
        /// [out] Upon return, is true if a vertical inertial scroll is recommended.
        /// false otherwise
        /// </param>
        /// <returns>
        /// Inertial scroll velocity.
        /// </returns>
        private Vector GetInertialScrollVelocity(Vector handPointerVelocity, out bool doHorizontalInertialScroll, out bool doVerticalInertialScroll)
        {
            // A positive hand pointer motion in X/Y means a negative scrolling motion towards the
            // beginning of the list.
            var scrollVelocity = -handPointerVelocity;

            doHorizontalInertialScroll = this.HorizontalScrollBarVisibility != ScrollBarVisibility.Disabled && Math.Abs(scrollVelocity.X) > MinSpeedForInertial;
            doVerticalInertialScroll = this.VerticalScrollBarVisibility != ScrollBarVisibility.Disabled && Math.Abs(scrollVelocity.Y) > MinSpeedForInertial;

            scrollVelocity.X = doHorizontalInertialScroll ? scrollVelocity.X : 0;
            scrollVelocity.Y = doVerticalInertialScroll ? scrollVelocity.Y : 0;

            return scrollVelocity;
        }

        /// <summary>
        /// Attempts to start an inertial scroll.
        /// </summary>
        /// <returns>
        /// true if inertial scroll was started. false otherwise (e.g.:
        /// we don't have enough data to compute a reliable velocity,
        /// velocity is too small to trigger an inertial scroll, etc.).
        /// </returns>
        private bool AttemptInertialScroll()
        {
            const int InertialScrollReleaseVelocityDirectionEndMs = 300;
            const int InertialScrollReleaseVelocityEndMsMax = 500;

            if ((int)(this.lastTimeStamp - this.lastGripTimeStamp) <= MinTimeSinceGripBeforeReleaseInertialScrollMs)
            {
                // We don't have enough hand pointer samples tracked to compute a reliable velocity
                return false;
            }

            var inertialScrollReleaseVelocityEndMs = (int)Math.Max(this.lastTimeStamp - (this.lastGripReleaseTimeStamp - GripDetectorDelayMs), InertialScrollReleaseVelocityEndMsMax);
            var inertialScrollReleaseDirectionEndMsFinal = Math.Min(InertialScrollReleaseVelocityDirectionEndMs, inertialScrollReleaseVelocityEndMs);

            var handPointerVelocity = this.sampleTracker.GetMaximumVelocity(
                GripDetectorDelayMs,
                inertialScrollReleaseVelocityEndMs,
                GripDetectorDelayMs,
                inertialScrollReleaseDirectionEndMsFinal,
                this.lastTimeStamp);

            bool doHorizontalInertialScroll, doVerticalInertialScroll;
            Vector scrollVelocity = this.GetInertialScrollVelocity(
                handPointerVelocity, out doHorizontalInertialScroll, out doVerticalInertialScroll);
            if (!doHorizontalInertialScroll && !doVerticalInertialScroll)
            {
                // We're not going to scroll in any direction, so bail out.
                return false;
            }

            this.scrollViewerInertiaScroller.Start(this, scrollVelocity, AnimationState.Inertial, doHorizontalInertialScroll, doVerticalInertialScroll);
            return true;
        }
    }
}