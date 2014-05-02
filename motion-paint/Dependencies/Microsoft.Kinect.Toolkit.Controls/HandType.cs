// -----------------------------------------------------------------------
// <copyright file="HandType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.Controls
{
    /// <summary>
    /// Used to denominate a specific hand of a user.
    /// </summary>
    public enum HandType
    {
        /// <summary>
        /// Refers to neither right nor left hand.
        /// </summary>
        None = 0,

        /// <summary>
        /// Refers to left hand.
        /// </summary>
        Left = 1,

        /// <summary>
        /// Refers to right hand.
        /// </summary>
        Right = 2
    }
}
