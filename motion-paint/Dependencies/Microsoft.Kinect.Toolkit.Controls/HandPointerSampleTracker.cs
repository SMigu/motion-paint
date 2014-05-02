// -----------------------------------------------------------------------
// <copyright file="HandPointerSampleTracker.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.Controls
{
    using System;
    using System.Windows;

    /// <summary>
    /// Helper class to take a list of position samples and return an average velocity.
    /// </summary>
    internal class HandPointerSampleTracker
    {
        private readonly int bufferSize;

        private readonly Sample[] samples;

        private int head;

        private int tail;

        /// <summary>
        /// Initializes a new instance of the HandPointerSampleTracker class.
        /// </summary>
        /// <param name="sampleCount">maximum number of samples to keep</param>
        public HandPointerSampleTracker(int sampleCount)
        {
            this.head = 0;
            this.tail = 0;
            this.bufferSize = sampleCount + 1;
            this.samples = new Sample[this.bufferSize];
        }

        /// <summary>
        /// Adds a sample
        /// </summary>
        /// <param name="x">x location</param>
        /// <param name="y">y location</param>
        /// <param name="timeStamp">timestamp, in milliseconds</param>
        public void AddSample(double x, double y, long timeStamp)
        {
            var sample = new Sample { X = x, Y = y, TimeStamp = timeStamp };

            this.samples[this.head] = sample;
            this.head = (this.head + 1) % this.bufferSize;
            if (this.head == this.tail)
            {
                // Circular queue is full, throw out an item
                this.tail = (this.tail + 1) % this.bufferSize;
            }
        }

        public void Clear()
        {
            this.head = this.tail;
        }

        /// <summary>
        /// Returns the average velocity of the sample history between minimum age and maximum age
        /// </summary>
        /// <param name="minAgeMs">minimum age of sample to consider</param>
        /// <param name="maxAgeMs">maximum age of sample to consider</param>
        /// <param name="presentTimestamp">timestamp representing the current frame</param>
        /// <returns>Velocity in units per second</returns>
        public Point GetAverageVelocity(int minAgeMs, int maxAgeMs, long presentTimestamp)
        {
            Point average, maxNegative, maxPositive;

            this.GetVelocityMetrics(minAgeMs, maxAgeMs, out average, out maxNegative, out maxPositive, presentTimestamp);

            return average;
        }

        /// <summary>
        /// Returns the maximum velocity of the sample history between minimum age and maximum age, filtering out samples that differ from the direction
        /// determined by analyzing the sample history between the minimum and maximum ages.
        /// </summary>
        /// <param name="minAgeMs">minimum age of sample to consider</param>
        /// <param name="maxAgeMs">maximum age of sample to consider</param>
        /// <param name="minAgeMsDirection">minimum age of sample to consider for determining direction</param>
        /// <param name="maxAgeMsDirection">maximum age of sample to consider for determining direction</param>
        /// <param name="presentTimestamp">timestamp representing the current frame</param>
        /// <returns>Velocity in units per second</returns>
        public Vector GetMaximumVelocity(int minAgeMs, int maxAgeMs, int minAgeMsDirection, int maxAgeMsDirection, long presentTimestamp)
        {
            Point average, maxNegative, maxPositive;

            this.GetVelocityMetrics(minAgeMsDirection, maxAgeMsDirection, out average, out maxNegative, out maxPositive, presentTimestamp);

            // determine direction
            bool positiveX = Math.Abs(maxPositive.X) > Math.Abs(maxNegative.X);
            bool positiveY = Math.Abs(maxPositive.Y) > Math.Abs(maxNegative.Y);

            this.GetVelocityMetrics(minAgeMs, maxAgeMs, out average, out maxNegative, out maxPositive, presentTimestamp);

            var outPoint = new Point
                             {
                                 X = positiveX ? maxPositive.X : maxNegative.X,
                                 Y = positiveY ? maxPositive.Y : maxNegative.Y
                             };

            return new Vector(outPoint.X, outPoint.Y);
        }

        /// <summary>
        /// Outputs the average, maximum positive, and maximum negative velocities over a time window
        /// </summary>
        /// <param name="minAgeMs">minimum age of sample to consider</param>
        /// <param name="maxAgeMs">maximum age of sample to consider</param>
        /// <param name="average">average velocity</param>
        /// <param name="maxNegative">maximum negative velocity</param>
        /// <param name="maxPositive">maximum positive velocity</param>
        /// <param name="presentTimestamp">timestamp representing the current frame</param>
        private void GetVelocityMetrics(int minAgeMs, int maxAgeMs, out Point average, out Point maxNegative, out Point maxPositive, long presentTimestamp)
        {
            int sumDivisor = 0;
            var sum = new Point(0.0, 0.0);
            maxNegative = new Point(0.0, 0.0);
            maxPositive = new Point(0.0, 0.0);

            int scan = this.head;

            while (scan != this.tail)
            {
                scan = (scan == 0) ? this.bufferSize - 1 : scan - 1;
                int next = (scan == 0) ? this.bufferSize - 1 : scan - 1;

                // if we've reached the head, we can't do another pair
                if (next == this.head)
                {
                    break;
                }

                long currentSampleSpan = presentTimestamp - this.samples[scan].TimeStamp;
                long nextSampleSpan = presentTimestamp - this.samples[next].TimeStamp;

                if (currentSampleSpan <= maxAgeMs && currentSampleSpan >= minAgeMs && nextSampleSpan <= maxAgeMs && nextSampleSpan >= minAgeMs)
                {
                    // convert from ms to seconds
                    double timeDelta = (this.samples[scan].TimeStamp - this.samples[next].TimeStamp) / 1000.0;

                    // don't calculate for samples that have the same timestamp
                    if (Math.Abs(timeDelta) > double.Epsilon)
                    {
                        double horzontalVelocity = (this.samples[scan].X - this.samples[next].X) / timeDelta;
                        double verticalVelocity = (this.samples[scan].Y - this.samples[next].Y) / timeDelta;

                        if (horzontalVelocity < maxNegative.X)
                        {
                            maxNegative.X = horzontalVelocity;
                        }

                        if (verticalVelocity < maxNegative.Y)
                        {
                            maxNegative.Y = verticalVelocity;
                        }

                        if (horzontalVelocity > maxPositive.X)
                        {
                            maxPositive.X = horzontalVelocity;
                        }

                        if (verticalVelocity > maxPositive.Y)
                        {
                            maxPositive.Y = verticalVelocity;
                        }

                        sum.X += horzontalVelocity;
                        sum.Y += verticalVelocity;

                        ++sumDivisor;
                    }
                }
            }

            if (sumDivisor < double.Epsilon)
            {
                average = new Point(0.0, 0.0);
            }
            else
            {
                sum.X /= sumDivisor;
                sum.Y /= sumDivisor;

                average = sum;
            }
        }

        private struct Sample
        {
            public long TimeStamp;

            public double X;
            public double Y;
        }
    }
}