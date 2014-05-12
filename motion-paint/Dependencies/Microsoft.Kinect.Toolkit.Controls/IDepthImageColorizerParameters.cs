// -----------------------------------------------------------------------
// <copyright file="IDepthImageColorizerParameters.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.Controls
{
    using System.Collections.Generic;
    using System.Windows.Media;

    /// <summary>
    /// Color settings used by DepthImageColorizerStrategy objects to determine
    /// how to highlight various users present in depth image.
    /// </summary>
    internal interface IDepthImageColorizerParameters
    {
        UserColoringMode UserColoringMode { get; }

        Color PrimaryUserColor { get; }

        IDictionary<int, Color> UserColors { get; }

        Color DefaultUserColor { get; }

        int PrimaryUserTrackingId { get; }

        int[] UserTrackingIds { get; } 
    }
}
