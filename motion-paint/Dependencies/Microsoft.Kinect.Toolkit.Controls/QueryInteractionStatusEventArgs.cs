// -----------------------------------------------------------------------
// <copyright file="QueryInteractionStatusEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.Controls
{
    using System.Windows;

    /// <summary>
    /// Event arguments for the KinectRegion.QueryInteractionStatusEvent
    /// </summary>
    public class QueryInteractionStatusEventArgs : HandPointerEventArgs
    {
        internal QueryInteractionStatusEventArgs(HandPointer handPointer, UIElement source)
            : base(handPointer, KinectRegion.QueryInteractionStatusEvent, source)
        {
        }

        public bool IsInGripInteraction { get; set; }
    }
}
