using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using RedEye.Util;

namespace RedEye
{
    public partial class MainWindow
    {
        private readonly Camera _camera;
        private readonly EnumerableImage _images;

        public MainWindow()
        {
            InitializeComponent();
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            _images = new EnumerableImage(@"assets\examples");
            _camera = new Camera(this);

            // Init Combobox:
            foreach (var filter in _camera.GetDevices()) DeviceBox.Items.Add(filter);
            DeviceBox.SelectedIndex = 0;

            RadioButtonImage.IsChecked = !(RadioButtonCam.IsChecked = Config.GetBool("StartWithCamera"));

            Console.WriteLine("CUDA " + (Config.IsCudaEnabled ? "On" : "Off"));
        }

        private void Window_OnClosed(object sender, EventArgs e)
        {
            _images.Dispose();
            _camera.Dispose();
            Detector.Dispose();
            Environment.Exit(Environment.ExitCode);
        }

        private void NextButton_OnClick(object? sender, RoutedEventArgs? e)
        {
            Pic.Source = _images.NextImage();
            Dispatcher.BeginInvoke((Action) DetectAsync, DispatcherPriority.ContextIdle);
        }

        private void RadioButtonImage_OnChecked(object sender, RoutedEventArgs e)
        {
            _camera.Dispose();
            Dispatcher.BeginInvoke((Action) SwitchToImage, DispatcherPriority.ContextIdle);
        }

        private void RadioButtonCamera_OnChecked(object sender, RoutedEventArgs e)
        {
            DeviceBox_OnSelectionChanged(null, null);
            NextButton.Visibility = Visibility.Hidden;
            DeviceBox.Visibility = Visibility.Visible;
        }

        private void DeviceBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs? e)
        {
            if (!RadioButtonCam.IsChecked ?? false) return;
            _camera.Dispose();
            _camera.Start(DeviceBox.SelectedIndex);
        }

        private void Window_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var window = (Window) sender;
            window.FontSize = window.ActualHeight / 35;
            MainCanvas.Height = window.ActualHeight / 2;
            RadioButtonGrid.Margin = new Thickness(
                window.ActualHeight / 20,
                0,
                window.ActualHeight / 20,
                window.ActualHeight / 100
            );
        }

        // Helper Methods:
        private void ClearCanvas()
        {
            MainCanvas.Children.Clear();
            MainCanvas.Children.Add(Pic);
        }

        public void DrawDetection(IOutputArrayOfArrays grayImage)
        {
            grayImage = ((Image<Gray, byte>) grayImage)
                .Resize(Math.Max((int) MainCanvas.ActualWidth, 1), (int) MainCanvas.ActualHeight, Inter.Linear);
            ClearCanvas();

            if (Config.GetBool("DrawRectangles"))
            {
                var rectangles = Detector.DetectAll(grayImage, Detector.DetectionObject.LeftEye)
                                         .Concat(Detector.DetectAll(grayImage, Detector.DetectionObject.RightEye));
                foreach (var rectangle in rectangles) MainCanvas.Children.Add(Detector.ConvertRectangle(rectangle));
            }

            if (Config.IsCudaEnabled)
            {
                MainCanvas.Children.Add(
                    ImageUtil.EyeTextureImage(
                        Detector.DetectCuda(grayImage, Detector.DetectionObject.LeftEye)));
                MainCanvas.Children.Add(
                    ImageUtil.EyeTextureImage(
                        Detector.DetectCuda(grayImage, Detector.DetectionObject.RightEye)));
            }
            else
            {
                MainCanvas.Children.Add(
                    ImageUtil.EyeTextureImage(
                        Detector.Detect(grayImage, Detector.DetectionObject.LeftEye),
                        MainCanvas.ActualWidth / 250));
                MainCanvas.Children.Add(
                    ImageUtil.EyeTextureImage(
                        Detector.Detect(grayImage, Detector.DetectionObject.RightEye),
                        MainCanvas.ActualWidth / 250));
            }
        }

        private void SwitchToImage()
        {
            Pic.Source = null;

            NextButton_OnClick(null, null);
            NextButton.Visibility = Visibility.Visible;
            DeviceBox.Visibility = Visibility.Hidden;
        }

        private void DetectAsync()
        {
            if (!Config.IsCudaEnabled) DrawDetection(new Mat(_images.CurrentImagePath()).ToImage<Gray, byte>());
            else DrawDetection(new GpuMat(new Mat(_images.CurrentImagePath()).ToImage<Gray, byte>()));
        }
    }
}