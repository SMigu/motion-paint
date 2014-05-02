// -----------------------------------------------------------------------
// <copyright file="DepthImageProcessor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using Microsoft.Kinect.Toolkit.Interaction;

    /// <summary>
    /// Used to process/render a depth bitmap efficiently.
    /// </summary>
    internal class DepthImageProcessor : DependencyObject, IDisposable, IDepthImageColorizerParameters
    {
        public static readonly DependencyProperty KinectSensorProperty = DependencyProperty.Register(
            "KinectSensor",
            typeof(KinectSensor),
            typeof(DepthImageProcessor),
            new FrameworkPropertyMetadata(null, (source, e) => ((DepthImageProcessor)source).KinectSensorPropertyChanged(e.OldValue as KinectSensor, e.NewValue as KinectSensor)));

        public static readonly DependencyProperty KinectRegionProperty = DependencyProperty.Register(
            "KinectRegion",
            typeof(KinectRegion),
            typeof(DepthImageProcessor),
            new FrameworkPropertyMetadata(
                null,
                (source, e) =>
                ((DepthImageProcessor)source).KinectRegionPropertyChanged(e.NewValue as KinectRegion)));

        public static readonly DependencyProperty WriteableBitmapProperty = DependencyProperty.Register(
            "WriteableBitmap",
            typeof(WriteableBitmap),
            typeof(DepthImageProcessor));

        private readonly DepthImageColorizerStrategy colorizerStrategy;
        
        private DepthImagePixel[] depthBuffer;
        private byte[] colorizedDepthBuffer;
        private int[] userTrackingIds;

        /// <summary>
        /// Initializes a new instance of the DepthImageProcessor class.
        /// </summary>
        public DepthImageProcessor()
        {
            this.colorizerStrategy = new DepthImageColorizerStrategy();
        }

        internal DepthImageProcessor(DepthImageColorizerStrategy colorizerStrategy)
        {
            this.colorizerStrategy = colorizerStrategy;
        }

        public event EventHandler<DepthImageProcessedEventArgs> ProcessedDepthImageReady;

        public KinectSensor KinectSensor
        {
            get { return (KinectSensor)this.GetValue(KinectSensorProperty); }
            set { this.SetValue(KinectSensorProperty, value); }
        }
        
        public KinectRegion KinectRegion
        {
            get { return (KinectRegion)this.GetValue(KinectRegionProperty); }
            set { this.SetValue(KinectRegionProperty, value); }
        }

        public int TargetWidth { get; set; }

        public int TargetHeight { get; set; }

        public UserColoringMode UserColoringMode { get; internal set; }

        public Color PrimaryUserColor { get; internal set; }

        public IDictionary<int, Color> UserColors { get; internal set; }

        public Color DefaultUserColor { get; internal set; }

        public int PrimaryUserTrackingId
        {
            get
            {
                return (this.KinectRegion != null)
                           ? this.KinectRegion.PrimaryUserTrackingId
                           : KinectPrimaryUserTracker.InvalidUserTrackingId;
            }
        }

        public int[] UserTrackingIds
        {
            get
            {
                if ((this.KinectRegion == null) || (this.KinectRegion.UserInfos == null))
                {
                    return null;
                }

                // Create a new array of tracking IDs if necessary
                if ((this.userTrackingIds == null) || (this.userTrackingIds.Length != this.KinectRegion.UserInfos.Length))
                {
                    this.userTrackingIds = new int[this.KinectRegion.UserInfos.Length];
                }

                // Copy latest user tracking Id information from KinectRegion.
                for (int i = 0; i < this.userTrackingIds.Length; ++i)
                {
                    this.userTrackingIds[i] = (this.KinectRegion.UserInfos[i] != null)
                                                  ? this.KinectRegion.UserInfos[i].SkeletonTrackingId
                                                  : KinectPrimaryUserTracker.InvalidUserTrackingId;
                }

                return this.userTrackingIds;
            }
        }

        public WriteableBitmap WriteableBitmap
        {
            get { return (WriteableBitmap)this.GetValue(WriteableBitmapProperty); }
            private set { this.SetValue(WriteableBitmapProperty, value); }
        }

        public void Dispose()
        {
            this.KinectSensorPropertyChanged(KinectSensor, null);
        }

        /// <summary>
        /// Process a frame and write it to the bitmap.
        /// </summary>
        public void WriteToBitmap(DepthImageFrame frame)
        {
            if ((null == this.depthBuffer) || (this.depthBuffer.Length != frame.PixelDataLength))
            {
                this.depthBuffer = new DepthImagePixel[frame.PixelDataLength];
                this.colorizedDepthBuffer = new byte[frame.PixelDataLength * 4];
            }

            if (null == WriteableBitmap || WriteableBitmap.Format != PixelFormats.Bgra32)
            {
                this.CreateWriteableBitmap(frame);
            }

            this.depthBuffer = frame.GetRawPixelData();
            
            this.colorizerStrategy.ColorizeDepthPixels(this, this.depthBuffer, this.colorizedDepthBuffer, frame.Width, frame.Height, (int)(frame.Width / WriteableBitmap.Width));

            this.WriteableBitmap.WritePixels(
                new Int32Rect(0, 0, WriteableBitmap.PixelWidth, WriteableBitmap.PixelHeight),
                this.colorizedDepthBuffer,
                (int)(WriteableBitmap.Width * 4),
                0);

            this.SendDepthImageReady(this.WriteableBitmap);
        }

        private static void EnforceAspectRatio(double targetAspectRatio, ref int width, ref int height)
        {
            var aspectRatio = (double)width / height;

            if (aspectRatio > targetAspectRatio)
            {
                // Input is too wide.
                width = (int)(height * aspectRatio);
            }
            else
            {
                // Input is too long.
                height = (int)(width / targetAspectRatio);
            }
        }

        private static void FindNextLargestFrameDimensions(DepthImageFrame frame, int targetWidth, int targetHeight, out int width, out int height)
        {
            width = frame.Width;
            height = frame.Height;

            if (frame.Width < targetWidth && frame.Height < targetHeight)
            {
                return;
            }

            while (width >= targetWidth * 2 && height >= targetHeight * 2 && width % 2 == 0 && height % 2 == 0)
            {
                width /= 2;
                height /= 2;
            }
        }

        private void CreateWriteableBitmap(DepthImageFrame frame)
        {
            int fixedTargetWidth = this.TargetWidth;
            int fixedTargetHeight = this.TargetHeight;

            int finalWidth;
            int finalHeight;

            EnforceAspectRatio((double)frame.Width / frame.Height, ref fixedTargetWidth, ref fixedTargetHeight);
            FindNextLargestFrameDimensions(frame, fixedTargetWidth, fixedTargetHeight, out finalWidth, out finalHeight);

            WriteableBitmap = new WriteableBitmap(
                finalWidth,
                finalHeight,
                96,
                96,
                PixelFormats.Bgra32,
                null);
        }

        private void KinectSensorPropertyChanged(KinectSensor oldSensor, KinectSensor newSensor)
        {
            if (oldSensor != null)
            {
                oldSensor.DepthFrameReady -= this.KinectSensorOnDepthFrameReady;
            }

            if (newSensor != null)
            {
                newSensor.DepthFrameReady += this.KinectSensorOnDepthFrameReady;
            }

            this.SendDepthImageReady(null);
        }

        private void KinectRegionPropertyChanged(KinectRegion newRegion)
        {
            if ((newRegion == null) && (this.colorizedDepthBuffer != null) && (this.WriteableBitmap != null))
            {
                // Clear color image to background color for as long as KinectRegion is invalid
                this.colorizerStrategy.ColorizeBackground(this.colorizedDepthBuffer);
                this.WriteableBitmap.WritePixels(
                    new Int32Rect(0, 0, WriteableBitmap.PixelWidth, WriteableBitmap.PixelHeight),
                    this.colorizedDepthBuffer,
                    (int)(WriteableBitmap.Width * 4),
                    0);

                this.SendDepthImageReady(this.WriteableBitmap);
            }
        }

        private void KinectSensorOnDepthFrameReady(object sender, DepthImageFrameReadyEventArgs depthImageFrameReadyEventArgs)
        {
            if ((this.KinectSensor != null) && (this.KinectRegion != null))
            {
                using (var frame = depthImageFrameReadyEventArgs.OpenDepthImageFrame())
                {
                    if (frame != null)
                    {
                        try
                        {
                            this.WriteToBitmap(frame);
                        }
                        catch (InvalidOperationException)
                        {
                            // DepthFrame functions may throw when the sensor gets
                            // into a bad state.  Ignore the frame in that case.
                        }
                    }
                }
            }
        }

        private void SendDepthImageReady(WriteableBitmap writeableBitmap)
        {
            if (this.ProcessedDepthImageReady != null)
            {
                this.ProcessedDepthImageReady(this, new DepthImageProcessedEventArgs { OutputBitmap = writeableBitmap });
            }
        }
    }

    internal class DepthImageProcessedEventArgs : EventArgs
    {
        public WriteableBitmap OutputBitmap { get; set; }
    }
}