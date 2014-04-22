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
        SettingsManager settings = new SettingsManager();
        private KinectSensorChooser sensorChooser;
        private InteractionStream _interactionStream;
        private Skeleton[] _skeletons; //the skeletons
        private UserInfo[] _userInfos; //the information about the interactive users
        private KinectSensor _sensor;
        private ControlManager controlManager = new ControlManager();
        private int paintingId = 0;
        public Color lastColor;
        public Color color = Colors.Black;
        private int thickness = 40;
        private string tool = "paintsplatter";

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;

            // initialize control modes
            controlManager.addControlMode(new OneHandMode(controlManager)); // id 0
            controlManager.addControlMode(new TwoHandMode(controlManager)); // id 1
            controlManager.changeCurrentControlMode(settings.controlModeId);

            //set the size of the canvas on load
            inkCanvas.Width = System.Windows.SystemParameters.PrimaryScreenWidth - 20;
            inkCanvas.Height = System.Windows.SystemParameters.PrimaryScreenHeight - 250;
            //set the widths of the bars on load
            BottomBar.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            colorBar.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            //set the size of the menu on load
            OuterMenuGrid.Width = System.Windows.SystemParameters.PrimaryScreenWidth - 10;
            OuterMenuGrid.Height = System.Windows.SystemParameters.PrimaryScreenHeight - 20;
            MenuGrid.Width = System.Windows.SystemParameters.PrimaryScreenWidth - 10;
            MenuGrid.Height = System.Windows.SystemParameters.PrimaryScreenHeight - 20;
            
            //Set size, color and margin of message popup
            popupMessageBar.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
                //set the bacground color of the pop up bar according to the type of the message
                // success: #FF70C763
                SolidColorBrush successBrush = new SolidColorBrush();
                successBrush.Color = Color.FromArgb(112, 199, 99, 255);
                // neutral: #FFF2FF71
                SolidColorBrush neutralBrush = new SolidColorBrush();
                neutralBrush.Color = Color.FromArgb(242, 255, 113, 255);
                // error: #FFCF513D
                SolidColorBrush errorBrush = new SolidColorBrush();
                errorBrush.Color = Color.FromArgb(207, 81, 61, 255);

            //Scaling if screen width is small (smaller than 1500), such as laptops
            if (System.Windows.SystemParameters.PrimaryScreenWidth < 1500)
            {
                //set the new width and height of the buttons
                brushButton.Width = 160; brushButton.Height = 120;
                paintsplatterButton.Width = 160; paintsplatterButton.Height = 120;
                patternButton.Width = 160; patternButton.Height = 120;
                eraserButton.Width = 160; eraserButton.Height = 120;
                newFileButton.Width = 160; newFileButton.Height = 120;
                openFileButton.Width = 160; openFileButton.Height = 120;
                SaveButton.Width = 160; SaveButton.Height = 120;
                ColorWheel.Width = 200; ColorWheel.Height = 200;
                SelectedColorShower.Width = 140; SelectedColorShower.Height = 140;
                MenuOpenBtn.Width = 100; MenuOpenBtn.Height = 100;
                BottomBar.Height = 150;

                //set the new margins of the buttons
                brushButton.Margin = new Thickness(10, 0, 0, 15);
                paintsplatterButton.Margin = new Thickness(170, 0, 0, 15);
                patternButton.Margin = new Thickness(330, 0, 0, 15);
                eraserButton.Margin = new Thickness(490, 0, 0, 15);
                newFileButton.Margin = new Thickness(0, 0, 180, 15);
                openFileButton.Margin = new Thickness(0, 0, 340, 15);
                SaveButton.Margin = new Thickness(0, 0, 500, 15);

                //Scale the color buttons as well
                ColorButton1.Width = 180; ColorButton2.Width = 180; ColorButton3.Width = 180;
                ColorButton4.Width = 180; ColorButton5.Width = 180; ColorButton6.Width = 180;
                ColorButton7.Width = 180; ColorButton8.Width = 180; ColorButton9.Width = 180;
                ColorButton10.Width = 180; ColorButton11.Width = 180; ColorButton12.Width = 180;
                //set the margins
                ColorButton1.Margin = new Thickness(10, 0, 0, 190);
                ColorButton2.Margin = new Thickness(190, 0, 0, 190);
                ColorButton3.Margin = new Thickness(370, 0, 0, 190);
                ColorButton4.Margin = new Thickness(560, 0, 0, 190);
                ColorButton5.Margin = new Thickness(740, 0, 0, 190);
                ColorButton6.Margin = new Thickness(920, 0, 0, 190);
                ColorButton7.Margin = new Thickness(10, 0, 0, 10);
                ColorButton8.Margin = new Thickness(190, 0, 0, 10);
                ColorButton9.Margin = new Thickness(370, 0, 0, 10);
                ColorButton10.Margin = new Thickness(560, 0, 0, 10);
                ColorButton11.Margin = new Thickness(740, 0, 0, 10);
                ColorButton12.Margin = new Thickness(920, 0, 0, 10);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Left;
            this.Top = desktopWorkingArea.Top;
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

        public Point oldPoint;
        public Point newPoint;
        bool stopDraw;

        private void InteractionStreamOnInteractionFrameReady(object sender, InteractionFrameReadyEventArgs e)
        {
            using (var iaf = e.OpenInteractionFrame())
            {
                if (iaf == null)
                    return;

                iaf.CopyInteractionDataTo(_userInfos);
            }

            // for activating and disabling draw
            if (controlManager.isDrawActive(_userInfos, _skeletons))
            {
                newPoint = controlManager.getCursorLocation(kinectRegion);

                if (oldPoint == null || stopDraw == true )
                {
                    oldPoint = newPoint;
                    stopDraw = false;
                }
                DrawCanvas.Paint(oldPoint, newPoint, inkCanvas, color, thickness, tool);
                oldPoint = newPoint;
            }
            else
            {
                stopDraw = true;
            }
        }

        private void MenuOpenBtn_Click(object sender, RoutedEventArgs e)
        {
            OuterMenuGrid.Visibility = System.Windows.Visibility.Visible;
            MenuGrid.Visibility = System.Windows.Visibility.Visible;
        }

        private void MenuCloseBtn_Click(object sender, RoutedEventArgs e)
        {
            OuterMenuGrid.Visibility = System.Windows.Visibility.Collapsed;
            MenuGrid.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void ColorWheel_Click(object sender, RoutedEventArgs e)
        {
            if (ColorSelectorGrid.Visibility == System.Windows.Visibility.Collapsed)
            {
                ColorSelectorGrid.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                ColorSelectorGrid.Visibility = System.Windows.Visibility.Collapsed;
            }
            
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            KinectTileButton b = sender as KinectTileButton;
            SolidColorBrush brush = b.Background as SolidColorBrush;
            lastColor = color;
            color = brush.Color;
            ColorSelectorGrid.Visibility = System.Windows.Visibility.Collapsed;
            SelectedColorShower.Fill = new SolidColorBrush(color);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var path = new Uri(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "/pic" + paintingId + ".png");
            Saveimage.ExportToPng(path, inkCanvas);
            popupMessages.Visibility = System.Windows.Visibility.Visible;
            MessageLabel.Content = "Tiedosto tallennettu";
        }

        private void MessageAcknowledged_Click(object sender, RoutedEventArgs e)
        {
            popupMessages.Visibility = System.Windows.Visibility.Collapsed;
            MessageLabel.Content = "";
        }

        // Clear canvas and increase painting id.
        private void newFileButton_Click(object sender, RoutedEventArgs e)
        {
            inkCanvas.Children.Clear();
            paintingId++;
        }

        private void ControlSelectButton_Click(object sender, RoutedEventArgs e) 
        {
            KinectTileButton b = sender as KinectTileButton;

            if (b.Name == "ControlSelect1")
            {
                controlManager.changeCurrentControlMode(0);
            }
            else 
            {
                controlManager.changeCurrentControlMode(1);                
            }
        }

        private void InputSelectButton_Click(object sender, RoutedEventArgs e) 
        {
        
        }
        
        private void brushButton_Click(object sender, RoutedEventArgs e)
        {
            tool = "brush";
            if (color == Colors.White)
                color = lastColor;
        }

        private void paintsplatterButton_Click(object sender, RoutedEventArgs e)
        {
            tool = "paintsplatter";
            if (color == Colors.White)
                color = lastColor;
        }

        private void eraserButton_Click(object sender, RoutedEventArgs e)
        {
            lastColor = color;
            color = Colors.White;
            tool = "brush";
        }
    }
}
