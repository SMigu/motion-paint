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
        private string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/pictures";
        private int offset = 0;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;

            // initialize control modes
            controlManager.addControlMode(new OneHandMode(controlManager)); // id 0
            controlManager.addControlMode(new TwoHandMode(controlManager)); // id 1
            controlManager.changeCurrentControlMode(settings.controlModeId);

            double width_ratio = (System.Windows.SystemParameters.PrimaryScreenWidth / 1920);
            double heigh_ratio = (System.Windows.SystemParameters.PrimaryScreenHeight / 1080);
            contentGrid.LayoutTransform = new ScaleTransform(width_ratio, heigh_ratio);

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

        private void UiButtonClick(object sender, RoutedEventArgs e)
        {
            String buttonName = "";
            if(sender as KinectTileButton != null)
            {
                buttonName = (sender as KinectTileButton).Name;
            }
            else 
            {
                if(sender as KinectCircleButton != null)
                {
                    buttonName = (sender as KinectCircleButton).Name;
                }
                else
                {
                    Console.WriteLine("Error: Button Element is invalid");
                }
            }

            switch (buttonName) 
            {
                case "MenuOpenBtn":
                    OuterMenuGrid.Visibility = System.Windows.Visibility.Visible;
                    MenuGrid.Visibility = System.Windows.Visibility.Visible;
                    break;
                case "MenuCloseBtn":
                    OuterMenuGrid.Visibility = System.Windows.Visibility.Collapsed;
                    MenuGrid.Visibility = System.Windows.Visibility.Collapsed;
                    break;

                case "ColorWheel":
                    if (ColorSelectorGrid.Visibility == System.Windows.Visibility.Collapsed)
                    {
                        ColorSelectorGrid.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        ColorSelectorGrid.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    break;
                case "SaveButton":
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
                    break;
                case "newFileButton":
                    inkCanvas.Children.Clear();
                    inkCanvas.Strokes.Clear();
                    paintingId++;
                    fileName = "";
                    break;

                case "brushButton":
                     tool = "brush";
                    if (color == Colors.White)
                        color = lastColor;
                    break;
                case "paintThrowButton":
                     tool = "throwpaint";
                    if (color == Colors.White)
                      color = lastColor;
                    break;
                case "eraserButton":
                    lastColor = color;
                    color = Colors.White;
                    tool = "brush";
                    break;
                case "patternButton":
                    if (patternMenu.Visibility == System.Windows.Visibility.Collapsed)
                    {
                        patternMenu.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        patternMenu.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    break;
                case "starButton":
                    tool = "starspray";
                    if (color == Colors.White)
                        color = lastColor;
                    patternMenu.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case "squareButton":
                    tool = "squarespray";
                    if (color == Colors.White)
                        color = lastColor;
                    patternMenu.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case "triangleButton":
                    tool = "trianglespray";
                    if (color == Colors.White)
                        color = lastColor;
                    patternMenu.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case "splatterButton":
                    tool = "paintsplatter";
                    if (color == Colors.White)
                        color = lastColor;
                    patternMenu.Visibility = System.Windows.Visibility.Collapsed;
                    break;

                case "thicknessIncreaseButton":
                    thickness += 10;
                    break;
                case "thicknessDecreaseButton":
                    if (thickness >= 20)
                    {
                        thickness -= 10;
                    }
                    break;

                case "openFileButton":
                    changeLoadMenuButtonBackgrounds();   
                    imageSelectOverlay.Visibility = System.Windows.Visibility.Visible;
                    break;
                case "nextImagesButton":
                    offset += 6;
                    if (!changeLoadMenuButtonBackgrounds())
                    {
                        offset -= 6;
                    }
                    break;
                case "previousImagesButton":
                     offset -= 6;
                    if (!changeLoadMenuButtonBackgrounds()) 
                    {
                        offset += 6;
                    }
                    break;
                case "closeOverlayButton":
                    imageSelectOverlay.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                default:
                    Console.WriteLine("Error: No click action found for element named, " + buttonName);
                    break;
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

        private void ControlSelectButton_Click(object sender, RoutedEventArgs e) 
        {
            KinectTileButton b = sender as KinectTileButton;

            if (b.Name == "ControlSelectButton1")
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

        private void loadPicture_Click(object sender, RoutedEventArgs e) 
        {
            KinectTileButton button = sender as KinectTileButton;
            string imgPath = (string)button.Tag;

            inkCanvas.Children.Clear();
            inkCanvas.Strokes.Clear();
            fileName = System.IO.Path.GetFileName(imgPath);
            Loadimage.ImportPng(new Uri(imgPath), inkCanvas);
            imageSelectOverlay.Visibility = Visibility.Collapsed;
        }
    }
}
