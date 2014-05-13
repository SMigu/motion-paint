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
using System.Windows.Media.Media3D;


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
            inkCanvas.Width = System.Windows.SystemParameters.PrimaryScreenWidth - 20;
            //inkCanvas.Height = System.Windows.SystemParameters.PrimaryScreenHeight - BottomBar.Height;
            
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

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
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
        public Point3D oldDepth;
        public Point3D newDepth;
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

                if (oldDepth == null || stopDraw == true)
                {
                    oldDepth = newDepth;

                }
                if (oldPoint == null || stopDraw == true )
                {
                    oldPoint = newPoint;
                    stopDraw = false;
                }

                newDepth = controlManager.getHandLocation();

                DrawCanvas.Paint(oldPoint, newPoint, inkCanvas, color, thickness, tool, oldDepth, newDepth);
                oldPoint = newPoint;
                oldDepth = newDepth;
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
                    brush.Color = Color.FromArgb(255, 224, 252, 213);
                    break;
                case "warning":
                    brush.Color = Color.FromArgb(255, 255, 246, 191);
                    break;
                case "error":
                    brush.Color = Color.FromArgb(255, 251, 227, 228);
                    break;
                default:
                    throw new ArgumentException("Wrong message type");
            }

            popupMessageBar.Fill = brush;
            MessageLabel.Content = msg;
            popupMessages.Visibility = System.Windows.Visibility.Visible;

            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 3);
            dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            MessageLabel.Content = "";
            popupMessages.Visibility = System.Windows.Visibility.Collapsed;
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
                foreach (KinectTileButton button in FindVisualChildren<KinectTileButton>(MainWindow1))
                {
                    KinectRegion.SetIsHoverTarget(button, false);
                    KinectRegion.SetIsPressTarget(button, true);
                }

                foreach (KinectCircleButton button in FindVisualChildren<KinectCircleButton>(MainWindow1))
                {
                    KinectRegion.SetIsHoverTarget(button, false);
                    KinectRegion.SetIsPressTarget(button, true);
                }

                settings.selectionMode = "push";
                settings.Save();
            }
            else
            {
                foreach (KinectTileButton button in FindVisualChildren<KinectTileButton>(MainWindow1))
                {
                    KinectRegion.SetIsHoverTarget(button, true);
                    KinectRegion.SetIsPressTarget(button, false);
                }

                foreach (KinectCircleButton button in FindVisualChildren<KinectCircleButton>(MainWindow1))
                {
                    KinectRegion.SetIsHoverTarget(button, true);
                    KinectRegion.SetIsPressTarget(button, false);
                }

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

        private void MainWindow1_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            //set the size of the canvas on Resize
            inkCanvas.Width = MainWindow1.Width - 20;
            inkCanvas.Height = MainWindow1.Height - BottomBar.Height - 150;

            if (MainWindow1.Height < 1000)
            {
                inkCanvas.Height = MainWindow1.Height - BottomBar.Height;
            }

            //set the widths of the bars on Resize
            BottomBar.Width = MainWindow1.Width;
            colorBar.Width = MainWindow1.Width;

            //set the size of the menu on Resize
            OuterMenuGrid.Width = MainWindow1.Width - 10;
            OuterMenuGrid.Height = MainWindow1.Height - 20;
            MenuGrid.Width = MainWindow1.Width - 100;
            MenuGrid.Height = MainWindow1.Height - 20;

            //Scaling if screen width is small (smaller than 1500), such as laptops
            if (MainWindow1.Width < 1500)
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

                inkCanvas.Height = MainWindow1.Height - BottomBar.Height - 40;

                patternMenu.Width = 160; patternMenuBar.Width = 160;
                triangleButton.Width = 140; squareButton.Width = 140; starButton.Width = 140;

                //set the new margins of the buttons
                brushButton.Margin = new Thickness(10, 0, 0, 15);
                paintsplatterButton.Margin = new Thickness(170, 0, 0, 15);
                patternButton.Margin = new Thickness(330, 0, 0, 15);
                eraserButton.Margin = new Thickness(490, 0, 0, 15);
                newFileButton.Margin = new Thickness(0, 0, 180, 15);
                openFileButton.Margin = new Thickness(0, 0, 340, 15);
                SaveButton.Margin = new Thickness(0, 0, 500, 15);

                patternMenu.Margin = new Thickness(330, 0, 0, 130);

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

        private void patternButton_Click(object sender, RoutedEventArgs e)
        {
            if (patternMenu.Visibility == System.Windows.Visibility.Collapsed)
            {
                patternMenu.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                patternMenu.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void starButton_Click(object sender, RoutedEventArgs e)
        {
            tool = "starspray";
            if (color == Colors.White)
                color = lastColor;

            patternMenu.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void squareButton_Click(object sender, RoutedEventArgs e)
        {
            tool = "squarespray";
            if (color == Colors.White)
                color = lastColor;

            patternMenu.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void triangleButton_Click(object sender, RoutedEventArgs e)
        {
            tool = "trianglespray";
            if (color == Colors.White)
                color = lastColor;

            patternMenu.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}
