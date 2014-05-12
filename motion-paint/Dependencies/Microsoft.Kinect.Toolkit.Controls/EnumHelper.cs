// -----------------------------------------------------------------------
// <copyright file="EnumHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.Controls
{
    using System.Diagnostics;

    using Microsoft.Kinect.Toolkit.Interaction;

    /// <summary>
    /// Utility class used internally to manipulate enumerations.
    /// </summary>
    internal static class EnumHelper
    {
        /// <summary>
        /// Convert an InteractionHandType value to corresponding HandType value.
        /// </summary>
        /// <param name="interactionHandType">
        /// InteractionHandType value to convert.
        /// </param>
        /// <returns>
        /// Corresponding HandType value.
        /// </returns>
        internal static HandType ConvertHandType(InteractionHandType interactionHandType)
        {
            switch (interactionHandType)
            {
                case InteractionHandType.Left:
                    return HandType.Left;

                case InteractionHandType.Right:
                    return HandType.Right;

                default:
                    Debug.Assert(interactionHandType == InteractionHandType.None, "HandType and InteractionHandType are out of sync.");
                    return HandType.None;
            }
        }

        /// <summary>
        /// Convert an InteractionHandEventType value to corresponding HandEventType value.
        /// </summary>
        /// <param name="interactionHandEventType">
        /// InteractionHandEventType value to convert.
        /// </param>
        /// <returns>
        /// Corresponding HandEventType value.
        /// </returns>
        internal static HandEventType ConvertHandEventType(InteractionHandEventType interactionHandEventType)
        {
            switch (interactionHandEventType)
            {
                case InteractionHandEventType.Grip:
                    return HandEventType.Grip;

                case InteractionHandEventType.GripRelease:
                    return HandEventType.GripRelease;

                default:
                    Debug.Assert(interactionHandEventType == InteractionHandEventType.None, "HandEventType and InteractionHandEventType are out of sync.");
                
                    return HandEventType.None;
            }
        }
    }
}
