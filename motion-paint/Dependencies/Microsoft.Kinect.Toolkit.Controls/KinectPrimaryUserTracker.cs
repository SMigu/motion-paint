// -----------------------------------------------------------------------
// <copyright file="KinectPrimaryUserTracker.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.Controls
{
    using System.Collections.Generic;

    /// <summary>
    /// Class that exposes and updates the current primary user. The primary user is defined as the first user with a primary pointer;
    /// they remain the primary user until they no longer have a primary pointer.
    /// </summary>
    internal class KinectPrimaryUserTracker
    {
        /// <summary>
        /// Defines an invalid tracking identifier.
        /// </summary>
        internal const int InvalidUserTrackingId = 0;

        internal KinectPrimaryUserTracker()
        {
            PrimaryUserTrackingId = InvalidUserTrackingId;
        }

        /// <summary>
        /// Gets the current primary user identifier
        /// </summary>
        internal int PrimaryUserTrackingId { get; private set; }

        /// <summary>
        /// Clear out the primary user being tracked.
        /// </summary>
        internal void Clear()
        {
            this.PrimaryUserTrackingId = InvalidUserTrackingId;
        }

        /// <summary>
        /// Iterate through all candidate users to determine which user is primary.
        /// The first user with a primary hand pointer (which means in the active region) is
        /// promoted to be primary user. 
        /// The primary user remains so until they no longer have a primary hand pointer.
        /// </summary>
        /// <param name="candidateHandPointers">
        /// Current list of available hand pointers derived from the interaction stream data.
        /// If this is null the function acts as if there are no tracked users and will end up
        /// with no primary user.
        /// </param>
        /// <param name="timestamp">
        /// Time of update. Corresponds to InteractionStream and KinectSensor event timestamps.
        /// </param>
        /// <param name="queryDelegate">
        /// Delegate used to ask client whether the proposed primary user tracking Id
        /// should be chosen or if an alternative tracking Id should be chosen instead.
        /// </param>
        internal void Update(IEnumerable<HandPointer> candidateHandPointers, long timestamp, QueryPrimaryUserTrackingIdCallback queryDelegate)
        {
            int firstPrimaryUserCandidate = InvalidUserTrackingId;
            int primaryUserTrackingId = InvalidUserTrackingId;
            bool currentPrimaryUserStillPrimary = false;

            if (candidateHandPointers != null)
            {
                var trackingIdsAvailable = new HashSet<int>();

                foreach (var handPointer in candidateHandPointers)
                {
                    if (handPointer.TrackingId == InvalidUserTrackingId)
                    {
                        continue;
                    }

                    trackingIdsAvailable.Add(handPointer.TrackingId);
                    
                    if (handPointer.IsPrimaryHandOfUser)
                    {
                        if (this.PrimaryUserTrackingId == handPointer.TrackingId)
                        {
                            // If the current primary user still has an active hand, we should continue to consider them the primary user.
                            currentPrimaryUserStillPrimary = true;
                        }
                        else if (InvalidUserTrackingId == firstPrimaryUserCandidate)
                        {
                            // Else if this is the first user with an active hand, they are the alternative candidate for primary user.
                            firstPrimaryUserCandidate = handPointer.TrackingId;
                        }
                    }
                }

                // Give client a chance to pick the primary user
                primaryUserTrackingId = currentPrimaryUserStillPrimary ? this.PrimaryUserTrackingId : firstPrimaryUserCandidate;
                if (queryDelegate != null)
                {
                    primaryUserTrackingId = queryDelegate(primaryUserTrackingId, candidateHandPointers, timestamp);
                }

                // If client specified an invalid tracking Id, silently fall back to the invalid tracking Id.
                if (!trackingIdsAvailable.Contains(primaryUserTrackingId))
                {
                    primaryUserTrackingId = InvalidUserTrackingId;
                }
            }

            this.PrimaryUserTrackingId = primaryUserTrackingId;
        }
    }
}
