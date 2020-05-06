using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Emgu.CV;
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

            // Init Images:
            _images = new EnumerableImage(@"assets\LFW");

            // Init Combobox:
            _camera = new Camera(this);
            foreach (var filter in _camera.GetDevices()) DeviceBox.Items.Add(filter);
            DeviceBox.SelectedIndex = 0;

            RadioButtonImage.IsChecked = !(RadioButtonCamera.IsChecked = Config.GetBool("StartWithCamera"));
        }

        // UI:
        private void Window_OnClosed(object sender, EventArgs e)
        {
            _images.Dispose();
            _camera.Dispose();
            Environment.Exit(Environment.ExitCode);
        }

        private void NextButton_OnClick(object sender, RoutedEventArgs e)
        {
            Pic.Source = _images.NextImage();
            var grayImage = new Mat(_images.CurrentImagePath()).ToImage<Gray, byte>();
            DrawDetection(grayImage);
        }

        private void RadioButtonImage_OnChecked(object sender, RoutedEventArgs e)
        {
            _camera.Dispose();
            Pic.Source = null;

            NextButton_OnClick(null, null);
            NextButton.Visibility = Visibility.Visible;
        }

        private void RadioButtonCamera_OnChecked(object sender, RoutedEventArgs e)
        {
            Pic.Source = null;
            ClearCanvas();

            NextButton.Visibility = Visibility.Hidden;
            DeviceBox_OnSelectionChanged(null, null);
        }

        private void ClearCanvas()
        {
            MainCanvas.Children.Clear();
            MainCanvas.Children.Add(Pic);
        }

        public void DrawDetection(IOutputArrayOfArrays grayImage)
        {
            ClearCanvas();
            MainCanvas.Children.Add(
                ImageUtil.EyeTextureImage(Detector.Detect(grayImage, Detector.DetectionObject.LeftEye))
            );
            MainCanvas.Children.Add(
                ImageUtil.EyeTextureImage(Detector.Detect(grayImage, Detector.DetectionObject.RightEye)));
        }

        private void DeviceBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!RadioButtonCamera.IsChecked ?? false) return;
            _camera.Dispose();
            _camera.Start(DeviceBox.SelectedIndex);
        }
    }
}