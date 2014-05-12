// -----------------------------------------------------------------------
// <copyright file="HandPointer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.Controls
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows;

    using Microsoft.Kinect.Toolkit.Interaction;

    /// <summary>
    /// Contains all the information we have about a hand pointer.
    /// </summary>
    public class HandPointer : INotifyPropertyChanged
    {
        private long timestampOfLastUpdate;

        private HandEventType handEventType;

        private bool isTracked;

        private bool isActive;

        private bool isInteractive;

        private bool isPressed;

        private bool isPrimaryHandOfUser;

        private bool isPrimaryUser;

        private bool isInGripInteraction;

        private UIElement captured;

        private double pressExtent;

        private HashSet<UIElement> enteredElements = new HashSet<UIElement>();

        /// <summary>
        /// Initializes a new instance of the <see cref="HandPointer"/> class. 
        /// internal constructor
        /// </summary>
        internal HandPointer()
        {
        }

        /// <summary>
        /// INotifyPropertyChanged implementation
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Skeletal tracking id of this HandPointer
        /// </summary>
        public int TrackingId { get; internal set; }

        /// <summary>
        /// Skeletal PlayerIndex id of this HandPointer
        /// </summary>
        public int PlayerIndex { get; internal set; }

        /// <summary>
        /// Left or right hand
        /// </summary>
        public HandType HandType { get; internal set; }

        /// <summary>
        /// Timestamp of Kinect sensor frame from which this HandPointer was
        /// last updated.
        /// </summary>
        public long TimestampOfLastUpdate
        {
            get
            {
                return this.timestampOfLastUpdate;
            }

            set
            {
                if (this.timestampOfLastUpdate != value)
                {
                    this.timestampOfLastUpdate = value;
                    this.OnPropertyChanged("TimestampOfLastUpdate");
                }
            }
        }

        /// <summary>
        /// Grip or GripRelease event
        /// </summary>
        public HandEventType HandEventType
        {
            get
            {
                return this.handEventType;
            }

            set
            {
                if (this.handEventType != value)
                {
                    this.handEventType = value;
                    this.OnPropertyChanged("HandEventType");
                }
            }
        }

        /// <summary>
        /// Set when this HandPointer belongs to a skeleton that is
        /// being actively tracked.
        /// </summary>
        public bool IsTracked
        {
            get
            {
                return this.isTracked;
            }

            set
            {
                if (this.isTracked != value)
                {
                    this.isTracked = value;
                    this.OnPropertyChanged("IsTracked");
                }
            }
        }

        /// <summary>
        /// Set when this HandPointer is in the active zone.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return this.isActive;
            }

            set
            {
                if (this.isActive != value)
                {
                    this.isActive = value;
                    this.OnPropertyChanged("IsActive");
                }
            }
        }

        /// <summary>
        /// Set when this HandPointer is in the interactive zone.
        /// </summary>
        public bool IsInteractive
        {
            get
            {
                return this.isInteractive;
            }

            set
            {
                if (this.isInteractive != value)
                {
                    this.isInteractive = value;
                    this.OnPropertyChanged("IsInteractive");
                }
            }
        }

        /// <summary>
        /// Set when this HandPointer is in a pressed state.
        /// </summary>
        public bool IsPressed
        {
            get
            {
                return this.isPressed;
            }

            set
            {
                if (this.isPressed != value)
                {
                    this.isPressed = value;
                    this.OnPropertyChanged("IsPressed");
                }
            }
        }

        /// <summary>
        /// Set when this is the primary hand of the user.  Some UI
        /// may only respond to the primary hand of the user.
        /// </summary>
        public bool IsPrimaryHandOfUser
        {
            get
            {
                return this.isPrimaryHandOfUser;
            }

            set
            {
                if (this.isPrimaryHandOfUser != value)
                {
                    this.isPrimaryHandOfUser = value;
                    this.OnPropertyChanged("IsPrimaryHandOfUser");
                }
            }
        }

        /// <summary>
        /// Set when this HandPointer belongs to the primary user.  Some UI
        /// may only respond to the primary hand of the user.
        /// </summary>
        public bool IsPrimaryUser
        {
            get
            {
                return this.isPrimaryUser;
            }

            set
            {
                if (this.isPrimaryUser != value)
                {
                    this.isPrimaryUser = value;
                    this.OnPropertyChanged("IsPrimaryUser");
                }
            }
        }

        /// <summary>
        /// Set when this HandPointer is involved in an operation
        /// that was initiated by a grip event.
        /// </summary>
        public bool IsInGripInteraction
        {
            get
            {
                return this.isInGripInteraction;
            }

            set
            {
                if (this.isInGripInteraction != value)
                {
                    this.isInGripInteraction = value;
                    this.OnPropertyChanged("IsInGripInteraction");
                }
            }
        }

        /// <summary>
        /// Element that has captured this HandPointer.  Null if not captured.
        /// </summary>
        public UIElement Captured
        {
            get
            {
                return this.captured;
            }

            set
            {
                if (this.captured != value)
                {
                    this.captured = value;
                    this.OnPropertyChanged("Captured");
                }
            }
        }

        /// <summary>
        /// How far the user has pressed. 
        /// </summary>
        public double PressExtent
        {
            get
            {
                return this.pressExtent;
            }

            set
            {
                if (this.pressExtent != value)
                {
                    this.pressExtent = value;
                    this.OnPropertyChanged("PressExtent");
                }
            }
        }

        internal HashSet<UIElement> EnteredElements
        {
            get
            {
                return this.enteredElements;
            }

            set
            {
                this.enteredElements = value;
            }
        }

        /// <summary>
        /// Helper to determine if this is the HandPointer being used by
        /// default for all control interaction.
        /// </summary>
        internal bool IsPrimaryHandOfPrimaryUser
        {
            get
            {
                return this.IsPrimaryUser && this.isPrimaryHandOfUser;
            }
        }

        /// <summary>
        /// Used to keep track if this cursor was updated in this
        /// frame.  Any cursor that wasn't updated will be removed.
        /// </summary>
        internal bool Updated { get; set; }

        /// <summary>
        /// X position of the cursor in WPF space, from left side of KinectRegion that
        /// owns this HandPointer.
        /// </summary>
        internal double X { get; set; }

        /// <summary>
        /// Y position of the cursor in WPF space, from top side of KinectRegion that
        /// owns this HandPointer.
        /// </summary>
        internal double Y { get; set; }

        /// <summary>
        /// KinectAdapter that manages the lifetime of this HandPointer.
        /// </summary>
        internal KinectAdapter Owner { get; set; }

        /// <summary>
        /// Gets the position of the hand pointer relative to a specified UI element.
        /// </summary>
        /// <param name="relativeTo">
        /// The element defining the coordinate space in which to calculate the position
        /// of the mouse. May be null.
        /// </param>
        /// <returns>
        /// The position of the hand pointer relative to the parameter
        /// <paramref name="relativeTo"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// If <paramref name="relativeTo"/> is null, position returned will treat top-left
        /// corner of KinectRegion that owns this hand pointer as (0,0).
        /// </para>
        /// <para>
        /// If <paramref name="relativeTo"/>.FlowDirection is LeftToRight, position returned
        /// will treat top-left corner of <paramref name="relativeTo"/> element as (0,0).
        /// </para>
        /// <para>
        /// If <paramref name="relativeTo"/>.FlowDirection is RightToLeft, position returned
        /// will treat top-right corner of <paramref name="relativeTo"/> element as (0,0).
        /// </para>
        /// </remarks>
        public Point GetPosition(UIElement relativeTo)
        {
            var position = new Point(this.X, this.Y);

            if (relativeTo == null)
            {
                return position;
            }

            var root = this.Owner.InteractionRootElement;

            if (root.FlowDirection == FlowDirection.RightToLeft)
            {
                position.X = root.ActualWidth - this.X;
            }

            return root.TranslatePoint(position, relativeTo);
        }

        /// <summary>
        /// Determines if this HandPointer is over a specific UIElement
        /// </summary>
        /// <param name="element">the element to check</param>
        /// <returns>true if this HandPointer is over the element.  False otherwise.</returns>
        public bool GetIsOver(UIElement element)
        {
            return this.enteredElements.Contains(element);
        }

        public bool Capture(UIElement element)
        {
            return this.Owner.CaptureHandPointer(this, element);
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}