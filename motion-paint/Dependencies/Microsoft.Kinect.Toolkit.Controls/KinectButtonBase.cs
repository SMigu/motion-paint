// -----------------------------------------------------------------------
// <copyright file="KinectButtonBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.Controls
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Automation.Peers;
    using System.Windows.Controls.Primitives;

    /// <summary>
    /// Button that responds to Kinect events
    /// </summary>
    [TemplateVisualState(Name = "Normal", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "MouseOver", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Pressed", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Disabled", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Focused", GroupName = "FocusStates")]
    [TemplateVisualState(Name = "Unfocused", GroupName = "FocusStates")]
    public abstract class KinectButtonBase : ButtonBase
    {
        private const double ReleaseRadius = 0.3;
        private const double ReleaseYCutoff = ReleaseRadius / 3.0;

        private static readonly bool IsInDesignMode = DesignerProperties.GetIsInDesignMode(new DependencyObject());

        private HandPointer capturedHandPointer;

        protected KinectButtonBase()
        {
            if (!IsInDesignMode)
            {
                this.InitializeKinectButtonBase();
            }
        }

        public void AutomationButtonBaseClick()
        {
            this.OnClick();
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new KinectButtonBaseAutomationPeer(this);
        }

        protected override void OnClick()
        {
            if (AutomationPeer.ListenerExists(AutomationEvents.InvokePatternOnInvoked))
            {
                AutomationPeer peer = UIElementAutomationPeer.CreatePeerForElement(this);
                if (peer != null)
                {
                    peer.RaiseAutomationEvent(AutomationEvents.InvokePatternOnInvoked);
                }
            }

            base.OnClick();
        }

        private void InitializeKinectButtonBase()
        {
            KinectRegion.AddHandPointerPressHandler(this, this.OnHandPointerPress);
            KinectRegion.AddHandPointerGotCaptureHandler(this, this.OnHandPointerCaptured);
            KinectRegion.AddHandPointerPressReleaseHandler(this, this.OnHandPointerPressRelease);
            KinectRegion.AddHandPointerLostCaptureHandler(this, this.OnHandPointerLostCapture);
            KinectRegion.AddHandPointerEnterHandler(this, this.OnHandPointerEnter);
            KinectRegion.AddHandPointerLeaveHandler(this, this.OnHandPointerLeave);

            KinectRegion.SetIsPressTarget(this, true);
        }

        private void OnHandPointerPress(object sender, HandPointerEventArgs handPointerEventArgs)
        {
            if (this.capturedHandPointer == null && handPointerEventArgs.HandPointer.IsPrimaryUser && handPointerEventArgs.HandPointer.IsPrimaryHandOfUser)
            {
                handPointerEventArgs.HandPointer.Capture(this);
                handPointerEventArgs.Handled = true;
            }
        }

        private void OnHandPointerCaptured(object sender, HandPointerEventArgs handPointerEventArgs)
        {
            if (this.capturedHandPointer == null)
            {
                this.capturedHandPointer = handPointerEventArgs.HandPointer;
                this.IsPressed = true;
                handPointerEventArgs.Handled = true;
            }
        }

        private void OnHandPointerPressRelease(object sender, HandPointerEventArgs handPointerEventArgs)
        {
            if (this.capturedHandPointer == handPointerEventArgs.HandPointer)
            {
                bool isWithinReleaseArea = true;

                // Remember what we need since after we release capture,
                // this.capturedHandPointer will be null
                var point = handPointerEventArgs.HandPointer.GetPosition(this);

                // point is relative to upper left of control, translate to center
                point.X -= this.ActualWidth / 2;
                point.Y -= this.ActualHeight / 2;

                double regionWidth = handPointerEventArgs.HandPointer.Owner.InteractionRootElement.ActualWidth;
                double regionHeight = handPointerEventArgs.HandPointer.Owner.InteractionRootElement.ActualHeight;

                // convert to screen space
                point.X /= regionWidth;
                point.Y /= regionHeight;

                handPointerEventArgs.HandPointer.Capture(null);

                if (point.Y < ReleaseYCutoff)
                {
                    isWithinReleaseArea = Math.Sqrt((point.X * point.X) + (point.Y * point.Y)) < ReleaseRadius;
                }

                if (isWithinReleaseArea)
                {
                    this.OnClick();
                    VisualStateManager.GoToState(this, "MouseOver", true);
                }
                else
                {
                    VisualStateManager.GoToState(this, "Normal", true);
                }

                handPointerEventArgs.Handled = true;
            }
        }

        private void OnHandPointerLostCapture(object sender, HandPointerEventArgs handPointerEventArgs)
        {
            if (this.capturedHandPointer == handPointerEventArgs.HandPointer)
            {
                this.capturedHandPointer = null;
                this.IsPressed = false;
                handPointerEventArgs.Handled = true;
            }
        }

        private void OnHandPointerEnter(object sender, HandPointerEventArgs handPointerEventArgs)
        {
            if (KinectRegion.GetIsPrimaryHandPointerOver(this))
            {
                VisualStateManager.GoToState(this, "MouseOver", true);
            }
        }

        private void OnHandPointerLeave(object sender, HandPointerEventArgs handPointerEventArgs)
        {
            if (!KinectRegion.GetIsPrimaryHandPointerOver(this))
            {
                VisualStateManager.GoToState(this, "Normal", true);
            }
        }
    }
}
