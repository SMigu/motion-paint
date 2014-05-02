// -----------------------------------------------------------------------
// <copyright file="KinectItemClickEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.Controls
{
    using System.Windows;

    /// <summary>
    /// Event args for the items control ItemClicked event
    /// </summary>
    internal class KinectItemClickEventArgs : RoutedEventArgs
    {
        public KinectItemClickEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source)
        {
            this.ClickedItem = source;
        }

        public object ClickedItem { get; private set; }
    }
}
