// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScrollViewerInertiaScroller.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;

    internal enum AnimationState
    {
        None, 

        Inertial,

        Stopping
    }

    /// <summary>
    /// Helper class to animate the viewport on a ScrollViewer using
    /// a very simplistic physics model.
    /// </summary>
    internal class ScrollViewerInertiaScroller
    {
        private const double Friction = 1000.0;

        private readonly DispatcherTimer timer = new DispatcherTimer();

        private AnimationState currentState = AnimationState.None;

        private DateTime lastTickTime;

        private int horizontalDirection;

        private int verticalDirection;

        private Vector currentSpeed;

        private ScrollViewer scrollViewer;

        private bool slowEnoughForSelection = true;

        private double rightScrollExtent;

        private double bottomScrollExtent;

        public ScrollViewerInertiaScroller()
        {
            // this does not necessarily guarantee fresh data per frame however.  this should update once per draw instead.
            this.timer.Interval = TimeSpan.FromMilliseconds(1000.0 / 120.0);
            this.timer.Tick += this.TimerOnTick;
        }

        public event EventHandler<EventArgs> SlowEnoughForSelectionChanged;

        public bool AnimatingHorizontally { get; set; }

        public bool AnimatingVertically { get; set; }

        public AnimationState AnimationState
        {
            get
            {
                return this.currentState;
            }
        }
        
        public double CurrentFriction { get; set; }

        public bool SlowEnoughForSelection
        {
            get
            {
                return this.slowEnoughForSelection;
            }

            set
            {
                bool oldValue = this.slowEnoughForSelection;
                this.slowEnoughForSelection = value;

                if ((oldValue != value) && (this.SlowEnoughForSelectionChanged != null))
                {
                    this.SlowEnoughForSelectionChanged(this, EventArgs.Empty);
                }
            }
        }

        public Vector BounceBackVelocity { get; set; }
        
        /// <summary>
        /// Adjusts a speed value to grow non-linearly above the pre-defined speed cutoff
        /// </summary>
        /// <param name="speed">speed to convert</param>
        /// <returns>converted speed</returns>
        public static double GetConvertedEffectiveSpeed(double speed)
        {
            const double SpeedCutoff = 1600.0;
            double finalSpeed = speed;

            if (finalSpeed > SpeedCutoff)
            {
                finalSpeed = SpeedCutoff + Math.Pow(finalSpeed - SpeedCutoff, 0.85);
            }

            return finalSpeed;
        }

        /// <summary>
        /// Start inertia scrolling
        /// </summary>
        /// <param name="view">View to animate</param>
        /// <param name="initialVelocity">starting velocity in units per second</param>
        /// <param name="state">the new animator state</param>
        /// <param name="canScrollHorizontal">enables horizontal scrolling</param>
        /// <param name="canScrollVertical">enables vertical scrolling</param>
        public void Start(
            ScrollViewer view, Vector initialVelocity, AnimationState state, bool canScrollHorizontal, bool canScrollVertical)
        {
            double newCurrentHorizontalVelocity = GetConvertedEffectiveSpeed(Math.Abs(initialVelocity.X));
            double newCurrentVerticalVelocity = GetConvertedEffectiveSpeed(Math.Abs(initialVelocity.Y));

            this.AnimatingHorizontally = canScrollHorizontal;
            this.AnimatingVertically = canScrollVertical;

            this.lastTickTime = DateTime.UtcNow;

            this.currentSpeed = new Vector(newCurrentHorizontalVelocity, newCurrentVerticalVelocity);
            this.SlowEnoughForSelection = false;
            this.CurrentFriction = Friction;
            this.horizontalDirection = Math.Sign(initialVelocity.X);
            this.verticalDirection = Math.Sign(initialVelocity.Y);
            this.currentState = state;
            this.scrollViewer = view;
            this.rightScrollExtent = this.scrollViewer.ExtentWidth - this.scrollViewer.ViewportWidth;
            this.bottomScrollExtent = this.scrollViewer.ExtentHeight - this.scrollViewer.ViewportHeight;
            this.timer.Start();
        }

        /// <summary>
        /// Sets current speed to the greater of the current speed and the supplied speed
        /// </summary>
        /// <param name="velocity">new velocity to be applied, if greater than current velocity</param>
        public void UpdateSpeedFromContinuousScrolling(Vector velocity)
        {
            var adjustedVelocity = new Vector(GetConvertedEffectiveSpeed(Math.Abs(velocity.X)), GetConvertedEffectiveSpeed(Math.Abs(velocity.Y)));

            if (this.currentState != AnimationState.Inertial)
            {
                return;
            }

            if (this.AnimatingHorizontally && (-Math.Sign(velocity.X) == this.horizontalDirection))
            {
                this.currentSpeed.X = Math.Max(adjustedVelocity.X, this.currentSpeed.X);
            }

            if (this.AnimatingVertically && (-Math.Sign(velocity.Y) == this.verticalDirection))
            {
                this.currentSpeed.Y = Math.Max(adjustedVelocity.Y, this.currentSpeed.Y);
            }         
        }

        /// <summary>
        /// Stop inertia scrolling
        /// </summary>
        public void Stop()
        {
            this.currentState = this.currentState == AnimationState.Inertial ? AnimationState.Stopping : AnimationState.None;
            this.currentSpeed = new Vector(0, 0);
            this.SlowEnoughForSelection = true;
        }

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            const double MinSpeed = 30.0;
            this.SlowEnoughForSelection = (this.currentSpeed.X <= MinSpeed) && (this.currentSpeed.Y <= MinSpeed);

            if (this.scrollViewer == null)
            {
                return;
            }

            DateTime currentTime = DateTime.UtcNow;
            double timeDelta = (currentTime - this.lastTickTime).TotalSeconds;
            this.lastTickTime = currentTime;

            if (this.currentState == AnimationState.None)
            {
                return;
            }

            if (this.currentState == AnimationState.Stopping)
            {
                this.timer.Stop();
                this.currentState = AnimationState.None;
                return;
            }

            // Figure out how far we should move the view
            Vector pixelsToMove = this.currentSpeed * timeDelta;
            pixelsToMove.X *= this.horizontalDirection;
            pixelsToMove.Y *= this.verticalDirection;

            var currentOffset = new Vector(scrollViewer.HorizontalOffset, scrollViewer.VerticalOffset);
            var newOffset = currentOffset + pixelsToMove;

            // Check the horizontal limits and snap to them when exceeded.
            Vector bounceBackVelocity = new Vector(0.0, 0.0);
            if (newOffset.X < 0)
            {
                // Reached left inner limit, so snap to it
                newOffset.X = 0;
                bounceBackVelocity.X = currentSpeed.X * this.horizontalDirection;
                this.currentSpeed.X = 0.0;
            }
            else if (newOffset.X > this.rightScrollExtent)
            {
                // Reached right inner limit so snap to it
                newOffset.X = this.rightScrollExtent;
                bounceBackVelocity.X = currentSpeed.X * this.horizontalDirection;
                this.currentSpeed.X = 0.0;
            }
            else
            {
                // Apply friction
                this.currentSpeed.X -= this.CurrentFriction * timeDelta;
            }

            // Check the vertical limits and snap to them when exceeded.
            if (newOffset.Y < 0)
            {
                // Reached top inner limit, so snap to it
                newOffset.Y = 0;
                bounceBackVelocity.Y = currentSpeed.Y * this.verticalDirection;
                this.currentSpeed.Y = 0.0;
            }
            else if (newOffset.Y > this.bottomScrollExtent)
            {
                // Reached bottom inner limit so snap to it
                newOffset.Y = this.bottomScrollExtent;
                bounceBackVelocity.Y = currentSpeed.Y * this.verticalDirection;
                this.currentSpeed.Y = 0.0;
            }
            else
            {
                // Apply friction
                this.currentSpeed.Y -= this.CurrentFriction * timeDelta;
            }

            this.BounceBackVelocity = bounceBackVelocity;

            if (this.AnimatingHorizontally)
            {
                this.scrollViewer.ScrollToHorizontalOffset(newOffset.X);
            }

            if (this.AnimatingVertically)
            {
                this.scrollViewer.ScrollToVerticalOffset(newOffset.Y);
            }

            if ((this.currentSpeed.X < MinSpeed) && (this.currentSpeed.Y < MinSpeed))
            {
                this.Stop();
            }
        }
    }
}