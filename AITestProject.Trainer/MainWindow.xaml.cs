using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using AITestProject.AI;
using AITestProject.AI.Data;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace AITestProject.Trainer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IEnumerator<string> _enumerable;
        private bool _leftEyeFound;
        private StreamWriter _writer;

        public MainWindow()
        {
            InitializeComponent();
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            DataPathBox.Text = Directory.GetCurrentDirectory() + @"\assets\ImageData.tsv";
        }

        private void Init(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            // File Stuff:
            var lines = 0;
            if (!File.Exists(DataPathBox.Text)) File.Create(DataPathBox.Text).Close();
            else lines = File.ReadLines(DataPathBox.Text).Count();

            _writer?.Dispose();
            _writer = new StreamWriter(new FileStream(DataPathBox.Text, FileMode.Append));

            // init:
            _enumerable = ImageData.ReadImagesFromFile(Directory.GetCurrentDirectory() + @"\assets\LFW\")
                                   .Skip(lines)
                                   .GetEnumerator();
            NextImage();
            _leftEyeFound = false;
        }

        private void Image_OnClick(object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition((Image) sender);

            if (!_leftEyeFound)
            {
                AppendToFile(point);
                _leftEyeFound = true;
                Console.Write("Left Eye:\t");
            }
            else
            {
                AppendToFile(point);
                _leftEyeFound = false;
                Console.Write("Right Eye:\t");
                NextImage();
            }

            Console.WriteLine("X: {0}, Y: {1}", (int) point.X, (int) point.Y);
        }

        // Helper Methods:
        private void NextImage()
        {
            if (!_enumerable.MoveNext()) throw new ArgumentException("This was the Last element.");
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

        private void AppendToFile(Point p)
        {
            if (!_leftEyeFound) _writer.Write(ImagePath() + ", " + (int) p.X + ", " + (int) p.Y);
            else
            {
                _writer.WriteLine(", " + (int) p.X + ", " + (int) p.Y);
                _writer.Flush();
            }
        }

        private void Dispose()
        {
            _enumerable.Dispose();
            _writer.Dispose();
        }

        // UI:
        private void OutPutDialog_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonSaveFileDialog
                         {
                             Title = "Select Output File",
                             DefaultFileName = "ImageData",
                             DefaultExtension = "tsv",
                             OverwritePrompt = false,
                             Filters = {new CommonFileDialogFilter("TSV-File", ".tsv")},
                             AlwaysAppendDefaultExtension = true,
                             DefaultDirectory = DataPathBox.Text,
                             InitialDirectory = DataPathBox.Text
                         };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok) DataPathBox.Text = dialog.FileName;

            dialog.Dispose();
        }

        private void Window_OnClosed(object sender, EventArgs e)
        {
            Dispose();
            Environment.Exit(Environment.ExitCode);
        }

        private void Start_OnClick(object sender, RoutedEventArgs e)
        {
            _writer.Dispose();
            var trainer = new Training();
        }

        private void SkipButton_OnClick(object sender, RoutedEventArgs e)
        {
            var file = new FileInfo(ImagePath());
            var replaceFolder = file.Directory?.FullName.Replace(@"\LFW\", @"\skip\");
            Console.WriteLine(replaceFolder);
            if (!Directory.Exists(replaceFolder)) Directory.CreateDirectory(replaceFolder);

            File.Move(file.FullName, replaceFolder + '\\' + file.Name, true);
            NextImage();
        }
    }
}