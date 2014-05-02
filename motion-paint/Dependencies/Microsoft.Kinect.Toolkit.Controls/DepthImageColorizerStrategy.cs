// -----------------------------------------------------------------------
// <copyright file="DepthImageColorizerStrategy.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.Controls
{
    using System.Windows.Media;

    internal class DepthImageColorizerStrategy
    {
        internal const int NumPlayersTracked = 6;
        private const int NoPlayerIndex = 0;

        private readonly int[] playerColorLookupTable;
        private readonly int backgroundColorInteger;

        public DepthImageColorizerStrategy()
        {
            // Initialize player color table.
            playerColorLookupTable = new int[NumPlayersTracked + 1];

            // Cache color integer values.
            // Background color is always transparent.
            this.backgroundColorInteger = GetBgraColorInt(Colors.Transparent);

            // Set color for "no player" to transparent.
            this.playerColorLookupTable[NoPlayerIndex] = this.backgroundColorInteger;
        }

        /// <summary>
        /// Fill a 32-bit RGBA image with appropriate colors given the specified parameters and
        /// depth image.
        /// </summary>
        /// <param name="parameters">
        /// Colorization parameters
        /// </param>
        /// <param name="depthImagePixels">
        /// Depth image pixels used to inform colorization process
        /// </param>
        /// <param name="colorBuffer">
        /// Color buffer to fill
        /// </param>
        /// <param name="depthWidth">
        /// Width of depth image (in pixels)
        /// </param>
        /// <param name="depthHeight">
        /// Height of depth image (in pixels)
        /// </param>
        /// <param name="downscaleFactor">
        /// Number of pixels in input depth image that correspond to one pixel in color image.
        /// </param>
        public virtual void ColorizeDepthPixels(IDepthImageColorizerParameters parameters, DepthImagePixel[] depthImagePixels, byte[] colorBuffer, int depthWidth, int depthHeight, int downscaleFactor)
        {
            int primaryColorInteger = GetBgraColorInt(parameters.PrimaryUserColor);
            int defaultColorInteger = GetBgraColorInt(parameters.DefaultUserColor);

            // Initialize lookup table to default color for all player indexes
            for (int i = 1; i < playerColorLookupTable.Length; i++)
            {
                playerColorLookupTable[i] = defaultColorInteger;
            }

            var trackingIds = parameters.UserTrackingIds;

            if (trackingIds != null)
            {
                // Iterate through user tracking Ids to populate color table.
                for (int i = 0; i < trackingIds.Length; ++i)
                {
                    // Index in user info array matches the zero-based index of corresponding
                    // skeleton within skeleton frame, while player indexes in depth image are
                    // shifted by one in order to be able to use zero as a marker to mean
                    // "pixel does not correspond to any player".
                    int depthPlayerIndex = i + 1;
                    var trackingId = trackingIds[i];

                    Color trackingIdColor;

                    switch (parameters.UserColoringMode)
                    {
                        case UserColoringMode.Manual:
                            if ((parameters.UserColors != null) && parameters.UserColors.TryGetValue(trackingId, out trackingIdColor))
                            {
                                // If UserColors contains the tracking ID for this user, use the specified color.
                                playerColorLookupTable[depthPlayerIndex] = GetBgraColorInt(trackingIdColor);
                            }

                        break;

                        case UserColoringMode.HighlightPrimary:
                            if ((parameters.PrimaryUserTrackingId != KinectPrimaryUserTracker.InvalidUserTrackingId)
                                && (parameters.PrimaryUserTrackingId == trackingId))
                            {
                                // If this is the primary user, use PrimaryUserColor.
                                playerColorLookupTable[depthPlayerIndex] = primaryColorInteger;
                            }

                        break;

                        //// Default: use DefaultUserColor for all users
                    }
                }
            }

            int pixelDisplacementBetweenRows = depthWidth * downscaleFactor;

            unsafe
            {
                fixed (byte* colorBufferPtr = colorBuffer)
                {
                    fixed (DepthImagePixel* depthImagePixelPtr = depthImagePixels)
                    {
                        fixed (int* playerColorLookupPtr = this.playerColorLookupTable)
                        {
                            // Write color values using int pointers instead of byte pointers,
                            // since each color pixel is 32-bits wide.
                            int* colorBufferIntPtr = (int*)colorBufferPtr;
                            DepthImagePixel* currentPixelRowPtr = depthImagePixelPtr;

                            for (int row = 0; row < depthHeight; row += downscaleFactor)
                            {
                                DepthImagePixel* currentPixelPtr = currentPixelRowPtr;
                                for (int column = 0; column < depthWidth; column += downscaleFactor)
                                {
                                    *colorBufferIntPtr++ = playerColorLookupPtr[currentPixelPtr->PlayerIndex];
                                    currentPixelPtr += downscaleFactor;
                                }

                                currentPixelRowPtr += pixelDisplacementBetweenRows;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Fill a 32-bit RGBA image with the background color.
        /// </summary>
        /// <param name="colorBuffer">
        /// Color buffer to fill
        /// </param>
        public virtual void ColorizeBackground(byte[] colorBuffer)
        {
            unsafe
            {
                fixed (byte* colorBufferPtr = colorBuffer)
                {
                    // Write color values using int pointers instead of byte pointers,
                    // since each color pixel is 32-bits wide.
                    int* colorBufferIntPtr = (int*)colorBufferPtr;

                    for (int pixel = 0; pixel < colorBuffer.Length; pixel += 4)
                    {
                        *colorBufferIntPtr++ = this.backgroundColorInteger;
                    }
                }
            }
        }

        private static int GetBgraColorInt(Color color)
        {
            return color.A << 24 | color.R << 16 | color.G << 8 | color.B;
        }
    }
}
