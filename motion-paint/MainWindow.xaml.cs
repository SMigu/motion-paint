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
using System.IO;


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
        private int thickness = 40;
        private string tool = "paintsplatter";
        private Color lastColor;
        private Color color = Colors.Black;
        private string fileName;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;

            // initialize control modes
            controlManager.addControlMode(new OneHandMode(controlManager)); // id 0
            controlManager.addControlMode(new TwoHandMode(controlManager)); // id 1
            controlManager.changeCurrentControlMode(settings.controlModeId);

            //set the size of the canvas on load
            inkCanvas.Width = System.Windows.SystemParameters.PrimaryScreenWidth - 200;
            inkCanvas.Height = System.Windows.SystemParameters.PrimaryScreenHeight - 250;
            //set the widths of the bars on load
            BottomBar.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            
            //Set size and margin of message popup
            popupMessageBar.Width = System.Windows.SystemParameters.PrimaryScreenWidth;

            //Scaling if screen width is small (smaller than 1500), such as laptops
            if (System.Windows.SystemParameters.PrimaryScreenWidth < 1500)
            {
                //set the new width and height of the buttons
                brushButton.Width = 180; brushButton.Height = 140;
                paintsplatterButton.Width = 180; paintsplatterButton.Height = 140;
                eraserButton.Width = 180; eraserButton.Height = 140;
                newFileButton.Width = 180; newFileButton.Height = 140;
                SaveButton.Width = 180; SaveButton.Height = 140;
                ColorWheel.Width = 200; ColorWheel.Height = 200;
                SelectedColorShower.Width = 140; SelectedColorShower.Height = 140;
                //set the new margins of the buttons
                paintsplatterButton.Margin = new Thickness(190, 0, 0, 10);
                eraserButton.Margin = new Thickness(370, 0, 0, 10);
                newFileButton.Margin = new Thickness(0, 0, 240, 10);
                SaveButton.Margin = new Thickness(0, 0, 460, 10);

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

            OuterMenuGrid.Width = System.Windows.SystemParameters.PrimaryScreenWidth - 10;
            OuterMenuGrid.Height = System.Windows.SystemParameters.PrimaryScreenHeight - 20;
            MenuGrid.Width = System.Windows.SystemParameters.PrimaryScreenWidth - 10;
            MenuGrid.Height = System.Windows.SystemParameters.PrimaryScreenHeight - 20;
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

        private void showMessage(string type, string msg) 
        {
            //set the bacground color of the pop up bar according to the type of the message
            SolidColorBrush brush = new SolidColorBrush();
            
            switch (type)
            {
                case "success":
                    // success: #FF70C763
                    brush.Color = Color.FromArgb(112, 199, 99, 255);
                    break;
                case "warning":
                    // neutral: #FFF2FF71
                    brush.Color = Color.FromArgb(242, 255, 113, 255);
                    break;
                case "error":
                    // error: #FFCF513D
                    brush.Color = Color.FromArgb(207, 81, 61, 255);
                    break;
                default:
                    throw new ArgumentException("Wrong message type");
            }

            popupMessageBar.Fill = brush;
            MessageLabel.Content = msg;
            popupMessages.Visibility = System.Windows.Visibility.Visible;   
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
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            Uri pictureUri;

            if (fileName == null || fileName == "")
            {
                fileName = string.Format("pic_{0:yyyy-MM-dd_hh-mm-ss-tt}.png", DateTime.Now);
                pictureUri = new Uri(path + System.IO.Path.DirectorySeparatorChar + fileName);
            }
            else 
            {
                try
                {
                    File.Delete(path + System.IO.Path.DirectorySeparatorChar + fileName);
                }
                catch (Exception ex)
                {
                    showMessage("error","Tiedoston tallennus epäonnistui");
                    Console.WriteLine(path + System.IO.Path.DirectorySeparatorChar + fileName + " " + ex.Message);
                    return;
                }
                fileName = string.Format("pic_{0:yyyy-MM-dd_hh-mm-ss-tt}.png", DateTime.Now);
                pictureUri = new Uri(path + System.IO.Path.DirectorySeparatorChar + fileName);
            }

            try
            {
                Saveimage.ExportToPng(pictureUri, inkCanvas);
            }
            catch (Exception ex)
            {
                showMessage("error", "Tiedoston tallennus epäonnistui");
                Console.WriteLine(pictureUri + " " + ex.Message);
                return;
            }    

            showMessage("success", "Tiedosto tallennettu");
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
            fileName = "";
        }

        private void ControlSelectButton_Click(object sender, RoutedEventArgs e) 
        {
            KinectTileButton b = sender as KinectTileButton;

            if (b.Name == "ControlSelect1")
            {
                controlManager.changeCurrentControlMode(0);
                settings.controlModeId = 0;
                settings.Save();
            }
            else 
            {
                controlManager.changeCurrentControlMode(1);
                settings.controlModeId = 1;
                settings.Save();
            }
        }

        private void InputSelectButton_Click(object sender, RoutedEventArgs e) 
        {
            KinectTileButton b = sender as KinectTileButton;

            if (b.Name == "InputSelect1")
            {
                settings.selectionMode = "push";
                settings.Save();
            }
            else
            {
                settings.selectionMode = "hover";
                settings.Save();
            }
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
