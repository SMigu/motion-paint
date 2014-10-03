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
        private string tool = "brush";
        private Color lastColor = Colors.Black;
        private Color color = Colors.Black;
        private string fileName;
        private string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/pictures" ;
        private int offset = 0;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;

            // initialize control modes
            controlManager.addControlMode(new OneHandMode(controlManager)); // id 0
            controlManager.addControlMode(new TwoHandMode(controlManager)); // id 1
            controlManager.changeCurrentControlMode(settings.controlModeId);       
            
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

            //set size of patternMenuBar onload
            patternMenuBar.Width = patternMenu.Width;
            patternMenuBar.Height = patternMenu.Height;

            //set the size of the canvas on load
            inkCanvas.Width = System.Windows.SystemParameters.PrimaryScreenWidth - 20;
            inkCanvas.Height = System.Windows.SystemParameters.PrimaryScreenHeight - BottomBar.Height -100; 

            //Scaling if screen width is small (smaller than 1500), such as laptops
            if (System.Windows.SystemParameters.PrimaryScreenWidth < 1500)
            {
                //set the new width and height of the buttons
                brushButton.Width = 160; brushButton.Height = 120;
                paintThrowButton.Width = 160; paintThrowButton.Height = 120;
                patternButton.Width = 160; patternButton.Height = 120;
                eraserButton.Width = 160; eraserButton.Height = 120;
                newFileButton.Width = 160; newFileButton.Height = 120;
                openFileButton.Width = 160; openFileButton.Height = 120;
                SaveButton.Width = 160; SaveButton.Height = 120;
                ColorWheel.Width = 200; ColorWheel.Height = 200;
                SelectedColorShower.Width = 140; SelectedColorShower.Height = 140;
                MenuOpenBtn.Width = 100; MenuOpenBtn.Height = 100;
                BottomBar.Height = 150;

                // Set the thickness adjust buttons
                thicknessUpButton.Width = 150; thicknessUpButton.Height = 150;
                thicknessDownButton.Width = 150; thicknessDownButton.Height = 150;

                // set the pattern menu buttons and menu
                triangleButton.Height = 120; triangleButton.Width = 160;
                squareButton.Height = 120; squareButton.Width = 160;
                starButton.Height = 120; starButton.Width = 160;
                splatterButton.Height = 120; splatterButton.Width = 160;

                patternMenu.Width = 180; patternMenu.Height = 520;
                patternMenuBar.Width = patternMenu.Width;
                patternMenuBar.Height = patternMenu.Height;

                //set the new margins of the pattern menu
                patternMenu.Margin = new Thickness(330, 0, 0, BottomBar.Height-5);
                triangleButton.Margin = new Thickness(10, 0, 0, 10);
                squareButton.Margin = new Thickness(10, 0, 0, 140);
                starButton.Margin = new Thickness(10, 0, 0, 270);
                splatterButton.Margin = new Thickness(10, 0, 0, 400);


                //set the new margins of the buttons
                brushButton.Margin = new Thickness(10, 0, 0, 15);
                paintThrowButton.Margin = new Thickness(170, 0, 0, 15);
                patternButton.Margin = new Thickness(330, 0, 0, 15);
                eraserButton.Margin = new Thickness(490, 0, 0, 15);
                newFileButton.Margin = new Thickness(0, 0, 180, 15);
                openFileButton.Margin = new Thickness(0, 0, 340, 15);
                SaveButton.Margin = new Thickness(0, 0, 500, 15);

                thicknessDownButton.Margin = new Thickness(0, 110, 0, 0);

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

                //set the inkCanvas again
                inkCanvas.Width = System.Windows.SystemParameters.PrimaryScreenWidth - 20;
                inkCanvas.Height = System.Windows.SystemParameters.PrimaryScreenHeight - BottomBar.Height - 60;   
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
                kinectRegion.Tag = "draw";
            }
            else
            {
                kinectRegion.Tag = "";
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

        private List<string> getImagePreviews(int amount, int offset)
        {
            List<string> retPics = new List<string>();
            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path);
                List<FileInfo> pictures = new List<FileInfo>();

                if (files != null || files.Length != 0)
                {
                    foreach (var file in files)
                    {
                        string ext = System.IO.Path.GetExtension(file);
                        if (ext == ".png")
                        {
                            FileInfo fInfo = new FileInfo(file);
                            pictures.Add(fInfo);
                        }
                    }

                    if (pictures.Count != 0) 
                    {
                        pictures = pictures.OrderByDescending(o => o.LastWriteTime).ToList();
                        for (int i = 0+offset; i < amount+offset; i++)
                        {
                            string picPath;
                            try
                            {
                                picPath = pictures[i].FullName;
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                picPath = "";
                            }          
                            
                            retPics.Add(picPath);
                        }
                    }
                }
            }
            return retPics;
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
            if (!System.IO.Directory.Exists(path)) 
            {
                System.IO.Directory.CreateDirectory(path);
            }

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
            inkCanvas.Strokes.Clear();
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

        private void paintThrowButton_Click(object sender, RoutedEventArgs e)
        {
            tool = "throwpaint";
            if (color == Colors.White)
                color = lastColor;
        }

        private void eraserButton_Click(object sender, RoutedEventArgs e)
        {
            lastColor = color;
            color = Colors.White;
            tool = "brush";
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

        private void paintsplatterButton_Click(object sender, RoutedEventArgs e)
        {
            tool = "paintsplatter";
            if (color == Colors.White)
                color = lastColor;

            patternMenu.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void thicknessAddButton_Click(object sender, RoutedEventArgs e) 
        {
            thickness += 10;
        }

        private void thicknessDecreaseButton_Click(object sender, RoutedEventArgs e)
        {
            if (thickness >= 20) 
            {
                thickness -= 10;
            }
        }

        private bool changeLoadMenuButtonBackgrounds() 
        {
            List<string> images = getImagePreviews(6, offset);
            string img = "";
            ImageBrush brush = null;
            if (images.Count == 0)
                return false;

            for (int i = 0; i < 6; i++)
            {
                if (images[i] != "")
                {
                    img = images[i];
                    brush = new ImageBrush();
                    brush.ImageSource = new BitmapImage(new Uri(img, UriKind.Absolute)).Clone();
                }
                else
                {
                    img = "";
                }

                switch (i)
                {
                    case 0:
                        if (img == "")
                        {
                            return false;
                        }
                        else
                        {
                            imageButton1.Tag = img;
                            imageButton1.Visibility = Visibility.Visible;
                            imageButton1.Background = brush;
                        }
                        break;
                    case 1:
                        if (img == "")
                        {
                            imageButton2.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            imageButton2.Tag = img;
                            imageButton2.Visibility = Visibility.Visible;
                            imageButton2.Background = brush;
                        }
                        break;
                    case 2:
                        if (img == "")
                        {
                            imageButton3.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            imageButton3.Tag = img;
                            imageButton3.Visibility = Visibility.Visible;
                            imageButton3.Background = brush;
                        }
                        break;
                    case 3:
                        if (img == "")
                        {
                            imageButton4.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            imageButton4.Tag = img;
                            imageButton4.Visibility = Visibility.Visible;
                            imageButton4.Background = brush;
                        }
                        break;
                    case 4:
                        if (img == "")
                        {
                            imageButton5.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            imageButton5.Tag = img;
                            imageButton5.Visibility = Visibility.Visible;
                            imageButton5.Background = brush;
                        }
                        break;
                    case 5:
                        if (img == "")
                        {
                            imageButton6.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            imageButton6.Tag = img;
                            imageButton6.Visibility = Visibility.Visible;
                            imageButton6.Background = brush;
                        }
                        break;
                    default:
                        break;
                }
            }
            return true;
        }

        private void openFileButton_Click(object sender, RoutedEventArgs e)
        {
            changeLoadMenuButtonBackgrounds();   
            imageSelectOverlay.Visibility = System.Windows.Visibility.Visible;
        }

        private void nextImagesButton_Click(object sender, RoutedEventArgs e) 
        {
            offset += 6;
            if (!changeLoadMenuButtonBackgrounds())
            {
                offset -= 6;
            }
        }

        private void previousImagesButton_Click(object sender, RoutedEventArgs e) 
        {
            offset -= 6;
            if (!changeLoadMenuButtonBackgrounds()) 
            {
                offset += 6;
            }
           
        }

        private void loadPicture_Click(object sender, RoutedEventArgs e) 
        {
            var button = (KinectTileButton)sender;
            string imgPath = (string)button.Tag;

            inkCanvas.Children.Clear();
            inkCanvas.Strokes.Clear();
            fileName = System.IO.Path.GetFileName(imgPath);
            Loadimage.ImportPng(new Uri(imgPath), inkCanvas);
            imageSelectOverlay.Visibility = Visibility.Collapsed;
        }

        private void closeOverlay_Click(object sender, RoutedEventArgs e)
        {
            imageSelectOverlay.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}
