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
            //set the size of the canvas on load
            inkCanvas.Width = System.Windows.SystemParameters.PrimaryScreenWidth - 200;
            inkCanvas.Height = System.Windows.SystemParameters.PrimaryScreenHeight - 250;
            //set the widths of the bars on load
            BottomBar.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            popupMessageBar.Width = System.Windows.SystemParameters.PrimaryScreenWidth;

            //Scaling if screen width is small (smaller than 1500), such as laptops
            if (System.Windows.SystemParameters.PrimaryScreenWidth < 1500)
            {
                //set the new width and height of the buttons
                Button1.Width = 180; Button1.Height = 140;
                Button2.Width = 180; Button2.Height = 140;
                Button3.Width = 180; Button3.Height = 140;
                FileButton.Width = 180; FileButton.Height = 140;
                SaveButton.Width = 180; SaveButton.Height = 140;
                ColorWheel.Width = 200; ColorWheel.Height = 200;
                SelectedColorShower.Width = 140; SelectedColorShower.Height = 140;
                //set the new margins of the buttons
                Button2.Margin = new Thickness(190, 0, 0, 10);
                Button3.Margin = new Thickness(370, 0, 0, 10);
                FileButton.Margin = new Thickness(0, 0, 240, 10);
                SaveButton.Margin = new Thickness(0, 0, 460, 10);

                //Scale the color buttons as well
                ColorButton1.Width = 180;
                ColorButton2.Width = 180;
                ColorButton3.Width = 180;
                ColorButton4.Width = 180;
                ColorButton5.Width = 180;
                ColorButton6.Width = 180;
                //set the margins
                ColorButton1.Margin = new Thickness(10, 0, 0, 209);
                ColorButton2.Margin = new Thickness(190, 0, 0, 209);
                ColorButton3.Margin = new Thickness(370, 0, 0, 209);
                ColorButton4.Margin = new Thickness(560, 0, 0, 209);
                ColorButton5.Margin = new Thickness(740, 0, 0, 209);
                ColorButton6.Margin = new Thickness(920, 0, 0, 209);
             
            }

            OuterMenuGrid.Width = System.Windows.SystemParameters.PrimaryScreenWidth - 10;
            OuterMenuGrid.Height = System.Windows.SystemParameters.PrimaryScreenHeight - 20;
            MenuGrid.Width = System.Windows.SystemParameters.PrimaryScreenWidth - 10;
            MenuGrid.Height = System.Windows.SystemParameters.PrimaryScreenHeight - 20;

            

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

        private void MenuOpenBtn_Click(object sender, RoutedEventArgs e)
        {
            OuterMenuGrid.Visibility = System.Windows.Visibility.Visible;
            //Menu.Visibility = System.Windows.Visibility.Visible;
            MenuGrid.Visibility = System.Windows.Visibility.Visible;
        }

        private void MenuCloseBtn_Click(object sender, RoutedEventArgs e)
        {
            OuterMenuGrid.Visibility = System.Windows.Visibility.Collapsed;
            //Menu.Visibility = System.Windows.Visibility.Collapsed;
            MenuGrid.Visibility = System.Windows.Visibility.Collapsed;

            
        }

        private void ColorWheel_Click(object sender, RoutedEventArgs e)
        {
            ColorSelectorGrid.Visibility = System.Windows.Visibility.Visible;
        }

        private void ColorButton1_Click(object sender, RoutedEventArgs e)
        {
            ColorSelectorGrid.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void ColorButton2_Click(object sender, RoutedEventArgs e)
        {
            ColorSelectorGrid.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void ColorButton3_Click(object sender, RoutedEventArgs e)
        {
            ColorSelectorGrid.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void ColorButton4_Click(object sender, RoutedEventArgs e)
        {
            ColorSelectorGrid.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void ColorButton5_Click(object sender, RoutedEventArgs e)
        {
            ColorSelectorGrid.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void ColorButton6_Click(object sender, RoutedEventArgs e)
        {
            ColorSelectorGrid.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            popupMessages.Visibility = System.Windows.Visibility.Visible;
            MessageLabel.Content = "Tiedosto tallennettu";
        }

        private void MessageAcknowledged_Click(object sender, RoutedEventArgs e)
        {
            popupMessages.Visibility = System.Windows.Visibility.Collapsed;
            MessageLabel.Content = "";
        }
        

    }
}
