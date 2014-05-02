// -----------------------------------------------------------------------
// <copyright file="KinectAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;

    using Microsoft.Kinect.Toolkit.Interaction;

    /// <summary>
    /// Helper class to provide common conversion operations between 
    /// Physical Interaction Zone and User Interface.
    /// </summary>
    internal static class InteractionZoneDefinition
    {
        /// <summary>
        /// Convert coordinates expressed in interaction zone coordinate space into
        /// coordinates relative to the area available for interaction in UI.
        /// </summary>
        /// <param name="izX">
        /// X-coordinate, where 0.0 corresponds to left side of interaction zone and
        /// 1.0 corresponds to right side of interaction zone.
        /// </param>
        /// <param name="izY">
        /// Y-coordinate, where 0.0 corresponds to left side of interaction zone and
        /// 1.0 corresponds to right side of interaction zone.
        /// </param>
        /// <param name="userInterfaceWidth">
        /// Width of UI area available for interaction.
        /// </param>
        /// <param name="userInterfaceHeight">
        /// Height of UI area available for interaction.
        /// </param>
        /// <param name="uiX">
        /// [out] X-coordinate, where 0.0 corresponds to left side of UI interaction
        /// area and <paramref name="userInterfaceWidth"/> corresponds to right side
        /// of UI interaction area.
        /// </param>
        /// <param name="uiY">
        /// [out] Y-coordinate, where 0.0 corresponds to top side of UI interaction
        /// area and <paramref name="userInterfaceHeight"/> corresponds to bottom side
        /// of UI interaction area.
        /// </param>
        public static void InteractionZoneToUserInterface(double izX, double izY, double userInterfaceWidth, double userInterfaceHeight, out double uiX, out double uiY)
        {
            uiX = userInterfaceWidth * izX;
            uiY = userInterfaceHeight * izY;
        }

        /// <summary>
        /// Convert coordinates relative to the area available for interaction in UI
        /// into coordinates expressed in interaction zone coordinate space.
        /// </summary>
        /// <param name="x">
        /// X-coordinate, where 0.0 corresponds to left side of UI interaction area
        /// and <paramref name="userInterfaceWidth"/> corresponds to right side of UI
        /// interaction area.
        /// </param>
        /// <param name="y">
        /// Y-coordinate, where 0.0 corresponds to top side of UI interaction area
        /// and <paramref name="userInterfaceHeight"/> corresponds to bottom side of UI
        /// interaction area.
        /// </param>
        /// <param name="userInterfaceWidth">
        /// Width of UI area available for interaction.
        /// </param>
        /// <param name="userInterfaceHeight">
        /// Height of UI area available for interaction.
        /// </param>
        /// <returns>
        /// A point in interaction zone coordinate space, where (0.0,0.0) corresponds
        /// to top-left corner and (1.0,1.0) corresponds to bottom-right corner.
        /// </returns>
        public static Point UserInterfaceToInteractionZone(double x, double y, double userInterfaceWidth, double userInterfaceHeight)
        {
            if (userInterfaceWidth <= 0 || userInterfaceHeight <= 0.0)
            {
                throw new ArgumentException("UI Width and Height must be greater than zero.");
            }

            return new Point { X = x / userInterfaceWidth, Y = y / userInterfaceHeight };
        }

        /// <summary>
        /// Convert coordinates expressed in interaction zone coordinate space into
        /// coordinates relative to the specified element.
        /// </summary>
        /// <param name="izX">
        /// X-coordinate, where 0.0 corresponds to left side of interaction zone and
        /// 1.0 corresponds to right side of interaction zone.
        /// 
        /// </param>
        /// <param name="izY">
        /// Y-coordinate, where 0.0 corresponds to top side of interaction zone and
        /// 1.0 corresponds to bottom side of interaction zone.
        /// Y-coordinate within <paramref name="element"/>, where 0.0 corresponds to
        /// top side and height of element corresponds to the bottom.
        /// </param>
        /// <param name="element">
        /// UI element that defines the input coordinate space.
        /// </param>
        /// <returns>
        /// A point relative to <paramref name="element"/>, where:
        /// <list type="bullet">
        /// <li>
        /// For X-coordinate, 0.0 corresponds to side used as horizontal origin
        /// and width of element corresponds to other side.
        /// Horizontal origin is on the left if FlowDirection is LeftToRight, and
        /// on the right if FlowDirection is RightToLeft.
        /// </li>
        /// <li>
        /// For X-coordinate, 0.0 corresponds to the top side and height of element
        /// corresponds to the bottom.
        /// </li>
        /// </list>
        /// </returns>
        public static Point InteractionZoneToElement(double izX, double izY, FrameworkElement element)
        {
            return new Point(
                element.ActualWidth * (element.FlowDirection == FlowDirection.LeftToRight ? izX : 1.0 - izX), element.ActualHeight * izY);
        }

        /// <summary>
        /// Convert coordinates relative to the specified element into coordinates
        /// expressed in interaction zone coordinate space.
        /// </summary>
        /// <param name="x">
        /// X-coordinate within <paramref name="element"/>, where 0.0 corresponds to
        /// side used as horizontal origin and width of element corresponds to other
        /// side.
        /// Horizontal origin is on the left if FlowDirection is LeftToRight, and
        /// on the right if FlowDirection is RightToLeft.
        /// </param>
        /// <param name="y">
        /// Y-coordinate within <paramref name="element"/>, where 0.0 corresponds to
        /// top side and height of element corresponds to the bottom.
        /// </param>
        /// <param name="element">
        /// UI element that defines the input coordinate space.
        /// </param>
        /// <returns>
        /// A point in interaction zone coordinate space, where (0.0,0.0) corresponds
        /// to top-left corner and (1.0,1.0) corresponds to bottom-right corner.
        /// </returns>
        public static Point ElementToInteractionZone(double x, double y, FrameworkElement element)
        {
            if (element.ActualWidth <= 0 || element.ActualHeight <= 0.0)
            {
                throw new ArgumentException("Element Width and Height must be greater than zero.");
            }

            double izX = x / element.ActualWidth;
            double izY = y / element.ActualHeight;
            return new Point { X = element.FlowDirection == FlowDirection.LeftToRight ? izX : 1.0 - izX, Y = izY };
        }

        /// <summary>
        /// Determines if user interface coordinate components are close enough to each other
        /// to be considered indistinguishable.
        /// </summary>
        /// <param name="a">
        /// First user interface coordinate component (i.e.: X or Y component).
        /// </param>
        /// <param name="b">
        /// Second user interface coordinate component (i.e.: X or Y component).
        /// </param>
        /// <returns>
        /// true if coordinate values are close enough to be considered indistinguishable.
        /// false otherwise
        /// </returns>
        public static bool AreUserInterfaceValuesClose(double a, double b)
        {
            return Math.Abs(a - b) < 0.5;
        }
    }

    /// <summary>
    /// Class to route KinectControls events to WPF controls
    /// </summary>
    internal class KinectAdapter : IInteractionClient
    {
        private const double PressTargetPointMargin = 0.0001;

        /// <summary>
        /// Mapping of the userId and hand to the hand pointer information
        /// </summary>
        private readonly Dictionary<Tuple<int, HandType>, HandPointer> handPointers =
            new Dictionary<Tuple<int, HandType>, HandPointer>();

        private readonly ObservableCollection<HandPointer> publicHandPointers = new ObservableCollection<HandPointer>();

        private readonly EventArgs emptyEventArgs = new EventArgs();

        /// <summary>
        /// Component that maintains and updates the current primary user.
        /// </summary>
        private readonly KinectPrimaryUserTracker kinectPrimaryUserTracker = new KinectPrimaryUserTracker();

        /// <summary>
        /// True if request to clear data for tracked hand pointers has not been serviced yet.
        /// </summary>
        private bool isClearRequestPending;

        /// <summary>
        /// Initializes a new instance of the <see cref="KinectAdapter"/> class. 
        /// </summary>
        public KinectAdapter(FrameworkElement interactionRootElement)
        {
            this.IsInInteractionFrame = false;
            this.InteractionRootElement = interactionRootElement;

            this.ReadOnlyPublicHandPointers = new ReadOnlyObservableCollection<HandPointer>(publicHandPointers);
        }

        internal event EventHandler<EventArgs> InternalHandPointersUpdated;

        public FrameworkElement InteractionRootElement { get; private set; }

        public ReadOnlyObservableCollection<HandPointer> ReadOnlyPublicHandPointers { get; private set; }

        public bool IsInInteractionFrame { get; private set; }


        public void BeginInteractionFrame()
        {
            Debug.Assert(!this.IsInInteractionFrame, "Call to BeginInteractionFrame made without call to EndInteractionFrame");
            
            this.IsInInteractionFrame = true;
            this.isClearRequestPending = false;
        
            foreach (var handPointer in this.handPointers.Values)
            {
                handPointer.Updated = false;
            }
        }

        /// <summary>
        /// Function that takes the state of the core interaction components
        /// and translates it to RoutedEvents.  Also updates hand pointer states.
        /// </summary>
        /// <param name="data">Data directly from the core interaction components</param>
        public void HandleHandPointerData(InteractionFrameData data)
        {
            Debug.Assert(this.IsInInteractionFrame, "Call to HandleHandPointerData made without call to BeginInteractionFrame");

            if (this.isClearRequestPending)
            {
                // We don't care about new hand pointer data if client requested to clear
                // all hand pointers while in the middle of interaction frame processing.
                return;
            }

            var id = new Tuple<int, HandType>(data.TrackingId, data.HandType);

            HandPointer handPointer;
            if (!this.handPointers.TryGetValue(id, out handPointer))
            {
                handPointer = new HandPointer
                {
                    TrackingId = data.TrackingId, 
                    PlayerIndex = data.PlayerIndex, 
                    HandType = data.HandType, 
                    Owner = this,
                };
                this.handPointers[id] = handPointer;
            }

            handPointer.Updated = true;

            handPointer.TimestampOfLastUpdate = data.TimeStampOfLastUpdate;
            handPointer.HandEventType = data.HandEventType;

            bool pressedChanged = handPointer.IsPressed != data.IsPressed;
            handPointer.IsPressed = data.IsPressed;

            handPointer.IsTracked = data.IsTracked;
            handPointer.IsActive = data.IsActive;
            handPointer.IsInteractive = data.IsInteractive;

            bool primaryHandOfPrimaryUserChanged = handPointer.IsPrimaryHandOfPrimaryUser != (data.IsPrimaryHandOfUser && data.IsPrimaryUser);
            handPointer.IsPrimaryHandOfUser = data.IsPrimaryHandOfUser;
            handPointer.IsPrimaryUser = data.IsPrimaryUser;

            double newX;
            double newY;
            InteractionZoneDefinition.InteractionZoneToUserInterface(data.X, data.Y, this.InteractionRootElement.ActualWidth, this.InteractionRootElement.ActualHeight, out newX, out newY);
            bool positionChanged = !InteractionZoneDefinition.AreUserInterfaceValuesClose(newX, handPointer.X) ||
                    !InteractionZoneDefinition.AreUserInterfaceValuesClose(newY, handPointer.Y) ||
                    !InteractionZoneDefinition.AreUserInterfaceValuesClose(data.Z, handPointer.PressExtent);
            handPointer.X = newX;
            handPointer.Y = newY;
            handPointer.PressExtent = data.Z;

            this.HandleHandPointerChanges(handPointer, pressedChanged, positionChanged, primaryHandOfPrimaryUserChanged, false);
        }

        public int ChoosePrimaryUser(long timestamp, int oldPrimaryUser, QueryPrimaryUserTrackingIdCallback callback)
        {
            // Stale hand pointers could confuse clients if they see them as part of callback parameters
            this.RemoveStaleHandPointers();

            this.kinectPrimaryUserTracker.Update(this.handPointers.Values, timestamp, callback);
            int newPrimaryUser = this.kinectPrimaryUserTracker.PrimaryUserTrackingId;

            if (oldPrimaryUser != newPrimaryUser)
            {
                // if primary user id has changed, update tracked pointers
                foreach (var handPointer in this.handPointers.Values)
                {
                    // If tracking id is valid and matches the new primary user,
                    // this hand pointer corresponds to the new primary user.
                    bool isPrimaryUser = (handPointer.TrackingId == newPrimaryUser) && (newPrimaryUser != KinectPrimaryUserTracker.InvalidUserTrackingId);

                    if (handPointer.IsPrimaryUser != isPrimaryUser)
                    {
                        bool oldIsPrimaryHandOfPrimaryUser = handPointer.IsPrimaryHandOfPrimaryUser;
                        handPointer.IsPrimaryUser = isPrimaryUser;

                        this.HandleHandPointerChanges(handPointer, false, false, oldIsPrimaryHandOfPrimaryUser != handPointer.IsPrimaryHandOfPrimaryUser, false);
                    }
                }
            }

            return newPrimaryUser;
        }

        public void EndInteractionFrame()
        {
            Debug.Assert(this.IsInInteractionFrame, "Call to EndInteractionFrame made without call to BeginInteractionFrame");

            this.RemoveStaleHandPointers();

            // Update the public list of hand pointers based on hand pointer tracking state
            foreach (var handPointer in this.handPointers.Values)
            {
                if (handPointer.IsTracked)
                {
                    if (!this.publicHandPointers.Contains(handPointer))
                    {
                        this.publicHandPointers.Add(handPointer);
                    }
                }
                else
                {
                    this.publicHandPointers.Remove(handPointer);
                }
            }

            this.InvokeHandPointersChanged();

            this.isClearRequestPending = false;
            this.IsInInteractionFrame = false;
        }

        public void ClearHandPointers()
        {
            this.kinectPrimaryUserTracker.Clear();

            if (!this.IsInInteractionFrame)
            {
                // If we're not already processing an interaction frame, we fake
                // an empty frame so that all hand pointers get cleared out.
                this.BeginInteractionFrame();
                this.EndInteractionFrame();
            }
            else
            {
                // If we're in the middle of processing an interaction frame, we
                // can't modify all of our data structures immediately, but need to
                // remember to do so.
                this.isClearRequestPending = true;
            }
        }

        public InteractionInfo GetInteractionInfoAtLocation(int skeletonTrackingId, InteractionHandType handType, double x, double y)
        {
            var interactionInfo = new InteractionInfo
            {
                IsPressTarget = false,
                IsGripTarget = false,
            };

            var hitTestPosition = InteractionZoneDefinition.InteractionZoneToElement(x, y, this.InteractionRootElement);

            Func<HandPointer, bool> isTargetCapturedElement =
                handPointer =>
                (handPointer.TrackingId == skeletonTrackingId)
                && (handPointer.HandType == EnumHelper.ConvertHandType(handType))
                && (handPointer.Captured != null);
            var targetHandPointer = this.publicHandPointers.FirstOrDefault(isTargetCapturedElement);
            var targetElement = targetHandPointer != null ? targetHandPointer.Captured : this.HitTest(hitTestPosition);

            if (targetElement != null)
            {
                // Walk up the tree and try to find a grip target and/or a press target
                for (DependencyObject search = targetElement;
                    search != null && search != this.InteractionRootElement &&
                    (!interactionInfo.IsGripTarget || !interactionInfo.IsPressTarget);
                    search = VisualTreeHelper.GetParent(search))
                {
                    var searchElement = search as FrameworkElement;
                    if (searchElement == null)
                    {
                        // We need ActualWidth and Height which comes
                        // with FrameworkElement 
                        continue;
                    }

                    if (!interactionInfo.IsPressTarget)
                    {
                        bool isPressTarget = KinectRegion.GetIsPressTarget(searchElement);
                        if (isPressTarget)
                        {
                            // We found a press target.
                            if (interactionInfo.PressTargetControlId == 0)
                            {
                                interactionInfo.PressTargetControlId = searchElement.GetHashCode();
                            }

                            interactionInfo.IsPressTarget = true;

                            // Apply the margin to the press target point
                            Point pressTargetPoint = ApplyControlPressPointMargin(KinectRegion.GetPressTargetPoint(searchElement));

                            // Convert from interaction zone space into actual control coordinates
                            var elementPressTargetPoint = InteractionZoneDefinition.InteractionZoneToElement(
                                pressTargetPoint.X, pressTargetPoint.Y, searchElement);

                            // Get it into the space of the KinectRegion
                            var regionPressTargetPoint = searchElement.TranslatePoint(elementPressTargetPoint, this.InteractionRootElement);

                            // Now put it into the interaction zone space but now relative to the KinectRegion
                            var interactionPressTargetPoint = InteractionZoneDefinition.ElementToInteractionZone(
                                regionPressTargetPoint.X, regionPressTargetPoint.Y, this.InteractionRootElement);

                            interactionInfo.PressAttractionPointX = interactionPressTargetPoint.X;
                            interactionInfo.PressAttractionPointY = interactionPressTargetPoint.Y;
                        }
                    }

                    if (!interactionInfo.IsGripTarget)
                    {
                        bool isGripTarget = KinectRegion.GetIsGripTarget(searchElement);

                        if (isGripTarget)
                        {
                            // We found a grip target.
                            interactionInfo.IsGripTarget = true;
                        }
                    }
                }
            }

            return interactionInfo;
        }

        internal bool CaptureHandPointer(HandPointer handPointer, UIElement element)
        {
            this.InteractionRootElement.VerifyAccess();

            var id = new Tuple<int, HandType>(handPointer.TrackingId, handPointer.HandType);
            HandPointer checkHandPointer;


            if (!this.handPointers.TryGetValue(id, out checkHandPointer))
            {
                // No hand pointer to capture
                return false;
            }

            if (!object.ReferenceEquals(handPointer, checkHandPointer))
            {
                // The HandPointer we have for this hand pointer is not the one
                // we were handed in.  Caller has an older instance of the HandPointer
                return false;
            }

            if (element != null && handPointer.Captured != null)
            {
                // Request wasn't to clear capture and some other element has this captured
                // Maybe this isn't necessary.
                return false;
            }

            SwitchCapture(handPointer, handPointer.Captured, element);

            return true;
        }

        private static HandPointerEventArgs CreateEventArgs(RoutedEvent routedEvent, UIElement targetElement, HandPointer handPointer)
        {
            return new HandPointerEventArgs(handPointer, routedEvent, targetElement);
        }
        
        /// <summary>
        /// Sends the events and updates the attached properties having to do
        /// with this hand pointer being over any elements.
        /// </summary>
        /// <param name="handPointer">The hand pointer we are working with</param>
        /// <param name="primaryHandOfPrimaryUserChanged"> Did the primary hand of the primary user change </param>
        /// <param name="oldIntersectedElements">The list of elements this hand pointer used to intersect</param>
        private static void DoIntersectionNotifications(HandPointer handPointer, bool primaryHandOfPrimaryUserChanged, HashSet<UIElement> oldIntersectedElements)
        {
            var wasPrimaryHandOfPrimaryUser = primaryHandOfPrimaryUserChanged ? !handPointer.IsPrimaryHandOfPrimaryUser : handPointer.IsPrimaryHandOfPrimaryUser;

            // Leave any elements we aren't in any more
            foreach (var oldIntersectedElement in oldIntersectedElements)
            {
                if (!handPointer.EnteredElements.Contains(oldIntersectedElement))
                {
                    // We are no longer over this element.

                    // If we were or still are the HandPointer for the primary user's
                    // primary hand then clear out the attatched DP for this.
                    if (wasPrimaryHandOfPrimaryUser)
                    {
                        KinectRegion.SetIsPrimaryHandPointerOver(oldIntersectedElement, false);
                    }

                    // Tell this element that this hand pointer has left
                    var leaveArgs = new HandPointerEventArgs(handPointer, KinectRegion.HandPointerLeaveEvent, oldIntersectedElement);
                    oldIntersectedElement.RaiseEvent(leaveArgs);
                }
                else
                {
                    if (wasPrimaryHandOfPrimaryUser && !handPointer.IsPrimaryHandOfPrimaryUser)
                    {
                        // Hand pointer didn't leave the element but it is no longer the primary
                        KinectRegion.SetIsPrimaryHandPointerOver(oldIntersectedElement, false);
                    }
                }
            }

            // Enter any elements that we are now in
            foreach (var newEnteredElement in handPointer.EnteredElements)
            {
                if (!oldIntersectedElements.Contains(newEnteredElement))
                {
                    // Tell this element the hand pointer entered.

                    // Set the attached DP for this
                    if (handPointer.IsPrimaryHandOfPrimaryUser)
                    {
                        KinectRegion.SetIsPrimaryHandPointerOver(newEnteredElement, true);
                    }

                    // Send the routed event for this.
                    var enterArgs = new HandPointerEventArgs(handPointer, KinectRegion.HandPointerEnterEvent, newEnteredElement);
                    newEnteredElement.RaiseEvent(enterArgs);
                }
                else
                {
                    if (!wasPrimaryHandOfPrimaryUser && handPointer.IsPrimaryHandOfPrimaryUser)
                    {
                        // Hand pointer was already in this element but it became the primary
                        KinectRegion.SetIsPrimaryHandPointerOver(newEnteredElement, true);
                    }
                }
            }
        }

        private static void SwitchCapture(HandPointer handPointer, UIElement oldElement, UIElement newElement)
        {
            handPointer.Captured = newElement;

            if (oldElement != null)
            {
                var lostArgs = CreateEventArgs(KinectRegion.HandPointerLostCaptureEvent, oldElement, handPointer);
                oldElement.RaiseEvent(lostArgs);
            }

            if (newElement != null)
            {
                var gotArgs = CreateEventArgs(KinectRegion.HandPointerGotCaptureEvent, newElement, handPointer);
                newElement.RaiseEvent(gotArgs);
            }
        }

        /// <summary>
        /// Applies the PressTargetPointMargin to the press target point.
        /// </summary>
        /// <param name="pressTargetPoint">The press target point we are given.</param>
        /// <returns>The updated press target point with the margin applied.</returns>
        private static Point ApplyControlPressPointMargin(Point pressTargetPoint)
        {
            pressTargetPoint.X = ApplyControlPressPointMargin(pressTargetPoint.X);
            pressTargetPoint.Y = ApplyControlPressPointMargin(pressTargetPoint.Y);
            return pressTargetPoint;
        }

        /// <summary>
        /// Takes in a coordinate and forces it into ranges [-Infinity, -PressTargetPointMargin], [PressTargetPointMargin, 1.0 - PressTargetPointMargin], or [PressTargetPointMargin, +Infinity]
        /// </summary>
        /// <param name="coordinate">The original coordinate.</param>
        /// <returns>The updated coordinate.</returns>
        private static double ApplyControlPressPointMargin(double coordinate)
        {
            if (coordinate >= 0 && coordinate <= 1.0)
            {
                return Math.Max(PressTargetPointMargin, Math.Min(coordinate, 1.0 - PressTargetPointMargin));
            }

            return coordinate < 0 ? Math.Min(coordinate, 0 - PressTargetPointMargin) : Math.Max(coordinate, 1.0 + PressTargetPointMargin);
        }

        private void HandleHandPointerChanges(
            HandPointer handPointer, bool pressedChanged, bool positionChanged, bool primaryHandOfPrimaryUserChanged, bool removed)
        {
            bool doPress = false;
            bool doRelease = false;
            bool doMove = false;
            bool doLostCapture = false;
            bool doGrip = false;
            bool doGripRelease = false;

            if (removed)
            {
                // Deny the existence of this hand pointer
                doRelease = handPointer.IsPressed;
                doLostCapture = handPointer.Captured != null;
            }
            else
            {
                if (pressedChanged)
                {
                    doPress = handPointer.IsPressed;
                    doRelease = !handPointer.IsPressed;
                }

                if (positionChanged)
                {
                    doMove = true;
                }

                doGrip = handPointer.HandEventType == HandEventType.Grip;
                doGripRelease = handPointer.HandEventType == HandEventType.GripRelease;
            }

            if (doLostCapture)
            {
                SwitchCapture(handPointer, handPointer.Captured, null);
            }

            var targetElement = handPointer.Captured;
            if (targetElement == null)
            {
                var position = handPointer.GetPosition(this.InteractionRootElement);
                targetElement = this.HitTest(position);
            }

            // Update internal enter/leave state
            HashSet<UIElement> oldIntersectingElements;
            this.UpdateIntersections(handPointer, removed ? null : targetElement, out oldIntersectingElements);

            // See if this hand pointer is participating in a grip-initiated
            // interaction.
            var newIsInGripInteraction = false;
            if (targetElement != null)
            {
                var args = new QueryInteractionStatusEventArgs(handPointer, this.InteractionRootElement);
                targetElement.RaiseEvent(args);

                if (args.Handled && args.IsInGripInteraction)
                {
                    newIsInGripInteraction = true;
                }
            }

            handPointer.IsInGripInteraction = newIsInGripInteraction;

            //// After this point there should be no more changes to the internal
            //// state of the handPointers.  We don't want event handlers calling us
            //// when our internal state is inconsistent.

            DoIntersectionNotifications(handPointer, primaryHandOfPrimaryUserChanged, oldIntersectingElements);

            if (targetElement == null)
            {
                return;
            }

            if (doGrip)
            {
                var args = CreateEventArgs(KinectRegion.HandPointerGripEvent, targetElement, handPointer);
                targetElement.RaiseEvent(args);
            }

            if (doGripRelease)
            {
                var args = CreateEventArgs(KinectRegion.HandPointerGripReleaseEvent, targetElement, handPointer);
                targetElement.RaiseEvent(args);
            }

            if (doPress)
            {
                var args = CreateEventArgs(KinectRegion.HandPointerPressEvent, targetElement, handPointer);
                targetElement.RaiseEvent(args);
            }

            if (doMove)
            {
                var args = CreateEventArgs(KinectRegion.HandPointerMoveEvent, targetElement, handPointer);
                targetElement.RaiseEvent(args);
            }

            if (doRelease)
            {
                var args = CreateEventArgs(KinectRegion.HandPointerPressReleaseEvent, targetElement, handPointer);
                targetElement.RaiseEvent(args);
            }
        }

        /// <summary>
        /// Helper to do a hit test in the KinectRegion.  If it hits a
        /// ContentElement it will walk up the logical tree to the
        /// UIElement that contains it.
        /// </summary>
        /// <param name="point">The offset coordinates within this element.</param>
        /// <returns>The child UIElement that is located at the given position.</returns>
        private UIElement HitTest(Point point)
        {
            var inputElement = this.InteractionRootElement.InputHitTest(point);
            var uiElement = inputElement as UIElement;

            // If we hit a UI element, we are done
            if (uiElement != null)
            {
                return uiElement;
            }

            // If we hit a ContentElement, try to find the UI
            // element that contains it.
            var contentElement = inputElement as ContentElement;
            while (contentElement != null)
            {
                var parent = LogicalTreeHelper.GetParent(contentElement);
                uiElement = parent as UIElement;
                if (uiElement != null)
                {
                    return uiElement;
                }

                contentElement = parent as ContentElement;
            }

            // Didn't hit anything we can work with.
            return null;
        }

        private void RemoveStaleHandPointers()
        {
            List<HandPointer> nonUpdatedHandpointers = null;

            foreach (var handPointer in this.handPointers.Values)
            {
                // If we need to stop tracking this hand pointer
                if (this.isClearRequestPending || !handPointer.Updated)
                {
                    if (nonUpdatedHandpointers == null)
                    {
                        nonUpdatedHandpointers = new List<HandPointer>();
                    }

                    nonUpdatedHandpointers.Add(handPointer);
                }
            }

            if (nonUpdatedHandpointers != null)
            {
                foreach (var handPointer in nonUpdatedHandpointers)
                {
                    var pressedChanged = handPointer.IsPressed;
                    const bool PositionChanged = false;
                    var primaryHandOfPrimaryUserChanged = handPointer.IsPrimaryHandOfPrimaryUser;
                    const bool Removed = true;

                    handPointer.IsTracked = false;
                    handPointer.IsActive = false;
                    handPointer.IsInteractive = false;
                    handPointer.IsPressed = false;
                    handPointer.IsPrimaryUser = false;
                    handPointer.IsPrimaryHandOfUser = false;

                    this.HandleHandPointerChanges(handPointer, pressedChanged, PositionChanged, primaryHandOfPrimaryUserChanged, Removed);

                    this.handPointers.Remove(new Tuple<int, HandType>(handPointer.TrackingId, handPointer.HandType));

                    this.publicHandPointers.Remove(handPointer);
                }
            }
        }

        private void InvokeHandPointersChanged()
        {
            if (this.InternalHandPointersUpdated != null)
            {
                this.InternalHandPointersUpdated(this.InteractionRootElement, this.emptyEventArgs);
            }
        }

        /// <summary>
        /// Updates the internal state about the elements this hand pointer is over.
        /// Does not set any attached properties or send any events.
        /// </summary>
        /// <param name="handPointer">The hand pointer we are updating</param>
        /// <param name="targetElement">element the hand pointer is over or captured to</param>
        /// <param name="oldIntersectedElements">The list of elements this hand pointer was intersecting before they were updated</param>
        private void UpdateIntersections(HandPointer handPointer, UIElement targetElement, out HashSet<UIElement> oldIntersectedElements)
        {
            var newIntersectedElements = new HashSet<UIElement>();
            var scanStop = (UIElement)VisualTreeHelper.GetParent(this.InteractionRootElement);

            oldIntersectedElements = handPointer.EnteredElements;

            // Get the list of elements this hand pointer is now over.  Note that
            // we will say the pointer is over an element even if it is really
            // over a child which could be transformed outside the bounds of
            // the item.
            for (DependencyObject scan = targetElement; scan != null && scan != scanStop; scan = VisualTreeHelper.GetParent(scan))
            {
                var scanElement = scan as UIElement;
                if (scanElement != null)
                {
                    newIntersectedElements.Add(scanElement);
                }
            }

            // Update to the new list of entered elements before sending
            // any events.  If we get called back in an event handler, we
            // will have the current data, not the old data.
            handPointer.EnteredElements = newIntersectedElements;
        }
    }
}
