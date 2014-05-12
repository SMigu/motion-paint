// -----------------------------------------------------------------------
// <copyright file="InteractionFrameData.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.Controls
{
    using System;
    using System.Windows;
    using Microsoft.Kinect.Toolkit.Interaction;

    /// <summary>
    /// Class to send hand pointer information we get from the UxCore components to
    /// the KinectAdaptor.
    /// </summary>
    internal class InteractionFrameData
    {
        public long TimeStampOfLastUpdate { get; set; }

        public int TrackingId { get; set; }

        public int PlayerIndex { get; set; }

        public HandType HandType { get; set; }

        public HandEventType HandEventType { get; set; }

        public bool IsTracked { get; set; }

        public bool IsActive { get; set; }

        public bool IsInteractive { get; set; }

        public bool IsPressed { get; set; }

        public bool IsPrimaryHandOfUser { get; set; }

        public bool IsPrimaryUser { get; set; }

        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }
    }
}