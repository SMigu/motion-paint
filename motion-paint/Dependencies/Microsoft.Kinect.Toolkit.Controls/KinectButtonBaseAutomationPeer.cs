// -----------------------------------------------------------------------
// <copyright file="KinectButtonBaseAutomationPeer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.Controls
{
    using System;
    using System.Windows.Automation.Peers;
    using System.Windows.Automation.Provider;
    using System.Windows.Threading;

    internal class KinectButtonBaseAutomationPeer : ButtonBaseAutomationPeer, IInvokeProvider
    {
        public KinectButtonBaseAutomationPeer(KinectButtonBase owner)
            : base(owner)
        {
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Invoke)
            {
                return this;
            }

            return base.GetPattern(patternInterface);
        }

        void IInvokeProvider.Invoke()
        {
            if (!this.IsEnabled())
            {
                throw new InvalidOperationException();
            }

            // Asynchronous call of click event
            // In ClickHandler opens a dialog and suspend the execution we don't want to block this thread
            Dispatcher.BeginInvoke(
                DispatcherPriority.Input,
                new DispatcherOperationCallback(delegate
                    {
                    ((KinectButtonBase)Owner).AutomationButtonBaseClick();
                    return null;
                }),
                null);
        }

        protected override string GetClassNameCore()
        {
            return "KinectButtonBase";
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Button;
        }
    }
}
