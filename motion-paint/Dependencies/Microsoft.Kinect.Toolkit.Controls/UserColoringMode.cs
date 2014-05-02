// -----------------------------------------------------------------------
// <copyright file="UserColoringMode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.Controls
{
    /// <summary>
    /// Coloring modes that indicate how the appropriate color for each user in scene
    /// will be determined by <see cref="KinectUserViewer"/>.
    /// </summary>
    public enum UserColoringMode
    {
        /// <summary>
        /// Primary user will be highlighted using KinectUserViewer.PrimaryUserColor
        /// and all other users will be colored with KinectUserViewer.DefaultUserColor.
        /// </summary>
        HighlightPrimary,

        /// <summary>
        /// Users specified in KinectUserViewer.UserColors will get colored as specified
        /// and all other users will be colored with KinectUserViewer.DefaultUserColor.
        /// </summary>
        Manual
    }
}
