using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using AITestProject.AI;

namespace AITestProject.Trainer
{
    public partial class MainWindow : Window
    {
        private readonly IEnumerator<string> _enumerable;

        public MainWindow()
        {
            InitializeComponent();
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            _enumerable = Util.GetImages(@"assets\LFW").GetEnumerator();
            NextImage();
        }

        // Helper Methods:
        private void NextImage()
        {
            if (!_enumerable.MoveNext()) throw new ArgumentException("This was the Last element.");
            Console.WriteLine(ImagePath());
            using var stream = new FileStream(ImagePath(), FileMode.Open);
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            Pic.Source = bitmapImage;
        }

        private string ImagePath()
        {
            return _enumerable.Current ?? throw new ArgumentException($"no pic found {_enumerable.Current}");
        }

        private void Dispose()
        {
            _enumerable.Dispose();
        }

        // UI:
        private void Window_OnClosed(object sender, EventArgs e)
        {
            Dispose();
            Environment.Exit(Environment.ExitCode);
        }

        private void NextButton_OnClick(object sender, RoutedEventArgs e)
        {
            NextImage();
        }
    }
}