// -----------------------------------------------------------------------
// <copyright file="HandEventType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.Controls
{
    /// <summary>
    /// Used to denominate an event triggered by some hand of some user.
    /// </summary>
    public enum HandEventType
    {
        /// <summary>
        /// No event has been triggered.
        /// </summary>
        None = 0,

        /// <summary>
        /// A hand has been gripped.
        /// </summary>
        Grip = 1,

        /// <summary>
        /// A hand's grip has been released.
        /// </summary>
        GripRelease = 2
    }
}
