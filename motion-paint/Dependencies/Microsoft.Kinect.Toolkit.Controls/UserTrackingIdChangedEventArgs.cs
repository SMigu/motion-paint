// -----------------------------------------------------------------------
// <copyright file="UserTrackingIdChangedEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.Controls
{
    using System;

    public sealed class UserTrackingIdChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserTrackingIdChangedEventArgs"/> class.
        /// </summary>
        /// <param name="oldValue">
        /// Old user tracking identifier.
        /// </param>
        /// <param name="newValue">
        /// New user tracking identifier.
        /// </param>
        public UserTrackingIdChangedEventArgs(int oldValue, int newValue)
            : base()
        {
            this.OldValue = oldValue;
            this.NewValue = newValue;
        }

        /// <summary>
        /// Old user tracking identifier
        /// </summary>
        public int OldValue { get; private set; }

        /// <summary>
        /// New user tracking identifier
        /// </summary>
        public int NewValue { get; private set; }
    }
}
