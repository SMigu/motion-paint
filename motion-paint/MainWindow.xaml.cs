using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Interaction;
using Microsoft.Kinect.Toolkit.Controls;


namespace motion_paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensorChooser sensorChooser;

        private InteractionStream _interactionStream;
        private Skeleton[] _skeletons; //the skeletons 
        private UserInfo[] _userInfos; //the information about the interactive users
        private KinectSensor _sensor;
        public Color color = Colors.Black;
        private int thickness = 40;
       
        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();
        }
        
        // Event handler Kinect controller change. Handles Sensor configs on start and stop.
        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {
            bool error = false;
            if (args.OldSensor != null)
            {
                try 
                {
                    args.OldSensor.DepthStream.Range = DepthRange.Default;
                    args.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.SkeletonStream.Disable();
                }
                catch (InvalidOperationException)
                {
                    error = true;
                }
            }

            if (args.NewSensor != null)
            {
                try
                {
                    var parameters = new TransformSmoothParameters
                    {
                        Smoothing = 0.3f,
                        Correction = 0.0f,
                        Prediction = 0.0f,
                        JitterRadius = 1.0f,
                        MaxDeviationRadius = 0.5f
                    };

                    _sensor = args.NewSensor;

                    _sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    _sensor.SkeletonStream.Enable(parameters);
                    _skeletons = new Skeleton[_sensor.SkeletonStream.FrameSkeletonArrayLength];
                    _userInfos = new UserInfo[InteractionFrame.UserInfoArrayLength];

                    try 
	                {	        
		                _sensor.DepthStream.Range = DepthRange.Default;
                        _sensor.SkeletonStream.EnableTrackingInNearRange = false;
                        _sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
	                }
	                catch (InvalidOperationException)
	                {
		                _sensor.DepthStream.Range = DepthRange.Default;
                        _sensor.SkeletonStream.EnableTrackingInNearRange = false;
		                error = true;
	                }

                    _interactionStream = new InteractionStream(_sensor, new DummyInteractionClient());
                    _interactionStream.InteractionFrameReady += InteractionStreamOnInteractionFrameReady;

                    _sensor.DepthFrameReady += SensorOnDepthFrameReady;
                    _sensor.SkeletonFrameReady += SensorOnSkeletonFrameReady;
                }
                catch(InvalidOperationException)
                {
                    error = true;
                }
            }

            if (!error)
            {
                kinectRegion.KinectSensor = _sensor;
            }
            else 
            {
                //tb.Text = "ERROR";
            }
        }

        private void SensorOnSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs skeletonFrameReadyEventArgs)
        {
            using (SkeletonFrame skeletonFrame = skeletonFrameReadyEventArgs.OpenSkeletonFrame())
            {
                if (skeletonFrame == null)
                {
                    return;
                }

                try
                {
                    skeletonFrame.CopySkeletonDataTo(_skeletons);
                    var accelerometerReading = _sensor.AccelerometerGetCurrentReading();
                    _interactionStream.ProcessSkeleton(_skeletons, accelerometerReading, skeletonFrame.Timestamp);
                }
                catch (InvalidOperationException)
                {
                    // If exception skip frame
                }
            }
        }

        private void SensorOnDepthFrameReady(object sender, DepthImageFrameReadyEventArgs depthImageFrameReadyEventArgs)
        {
            using (DepthImageFrame depthFrame = depthImageFrameReadyEventArgs.OpenDepthImageFrame())
            {
                if (depthFrame == null)
                {
                    return;
                }

                try
                {
                    _interactionStream.ProcessDepth(depthFrame.GetRawPixelData(), depthFrame.Timestamp);
                }
                catch (InvalidOperationException)
                {
                    // If exception skip frame
                }
            }
        }

        private Dictionary<int, InteractionHandEventType> _lastLeftHandEvents = new Dictionary<int, InteractionHandEventType>();
        private Dictionary<int, InteractionHandEventType> _lastRightHandEvents = new Dictionary<int, InteractionHandEventType>();
        private Dictionary<int, bool> _lastActiveLeftHands = new Dictionary<int, bool>();
        private Dictionary<int, bool> _lastActiveRightHands = new Dictionary<int, bool>();
        private Dictionary<int, Point> _lastLeftHandPositions = new Dictionary<int, Point>();
        private Dictionary<int, Point> _lastRightHandPositions = new Dictionary<int, Point>();
        public Point oldPoint;
        public Point newPoint;
        public double screenX;
        public double screenY;
        bool stopDraw;
        
        // Id of the primary user. 
        // TODO Make better chooser 
        int primaryUserId = 1;
        public Vector _primaryPointerPosition;

        private void InteractionStreamOnInteractionFrameReady(object sender, InteractionFrameReadyEventArgs e)
        {
            using (var iaf = e.OpenInteractionFrame())
            {
                if (iaf == null)
                    return;

                iaf.CopyInteractionDataTo(_userInfos);
            }

            var hasUser = false;
            foreach (var userInfo in _userInfos)
            {
                var userID = userInfo.SkeletonTrackingId;
                if (userID == 0)
                {
                    continue;
                }
                hasUser = true;
                var hands = userInfo.HandPointers;
                if (hands.Count == 0)
                {
                }
                else
                {
                    foreach (var hand in hands)
                    {
                        var lastHandEvents = hand.HandType == InteractionHandType.Left ? _lastLeftHandEvents : _lastRightHandEvents;
                        var lastActiveHands = hand.HandType == InteractionHandType.Left ? _lastActiveLeftHands : _lastActiveRightHands;
                        var lastHandPositions = hand.HandType == InteractionHandType.Left ? _lastLeftHandPositions : _lastRightHandPositions;

                        if (hand.HandEventType != InteractionHandEventType.None)
                        {
                            lastHandEvents[userID] = hand.HandEventType;
                        }
                        lastActiveHands[userID] = hand.IsActive;
                        lastHandPositions[userID] = new Point(hand.X, hand.Y);
                        
                        screenX = hand.X * kinectRegion.ActualWidth;
                        screenY = hand.Y * kinectRegion.ActualHeight;
                        _primaryPointerPosition = new Vector(screenX, screenY);

                    }
                    primaryUserId = userID;
                }
                
                //tb.Text = _lastActiveLeftHands[userID].ToString() + " " + _lastActiveRightHands[userID];
            }

            // for activating and disabling draw (two hand draw mode)
            if (_lastLeftHandPositions.ContainsKey(primaryUserId) && _lastLeftHandPositions[primaryUserId].Y < 1.2 && _lastActiveRightHands.ContainsKey(primaryUserId) && _lastActiveRightHands[primaryUserId])
            {
                newPoint = new Point (screenX, screenY);

                if (oldPoint == null || stopDraw == true )
                {
                    oldPoint = newPoint;
                    stopDraw = false;
                }
                DrawCanvas.Paint(oldPoint,newPoint,inkCanvas, color, thickness);
                oldPoint = newPoint;
            }
            else
            {
                stopDraw = true;
            }

            if (!hasUser)
            {
            }
        }

        private void clearOnClick(object sender, RoutedEventArgs e)
        {
            inkCanvas.Children.Clear();
        }

        private void blueOnClick(object sender, RoutedEventArgs e)
        {
            color = Colors.Blue;
        }

        private void blackOnClick(object sender, RoutedEventArgs e)
        {
            color = Colors.Black;
        }
        private void redOnClick(object sender, RoutedEventArgs e)
        {
            color = Colors.Red;
        }
        private void greenOnClick(object sender, RoutedEventArgs e)
        {
            color = Colors.Green;
        }
        private void increaseSizeOnClick(object sender, RoutedEventArgs e) 
        {
            thickness += 10;
        }
        private void decreaseSizeOnClick(object sender, RoutedEventArgs e)
        {
            if (thickness != 10)
            {
                thickness -= 10;
            }
        }
    }
}
