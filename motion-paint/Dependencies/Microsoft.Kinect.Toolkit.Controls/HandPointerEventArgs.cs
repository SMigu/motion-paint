// -----------------------------------------------------------------------
// <copyright file="HandPointerEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.Controls
{
    using System.Windows;

    public class HandPointerEventArgs : RoutedEventArgs
    {
        internal HandPointerEventArgs(HandPointer handPointer, RoutedEvent routedEvent, UIElement source)
            : base(routedEvent, source)
        {
            this.HandPointer = handPointer;
        }

        /// <summary>
        /// HandPointer associated with event
        /// </summary>
        public HandPointer HandPointer { get; private set; }
    }
}
