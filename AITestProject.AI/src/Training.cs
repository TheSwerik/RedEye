using System;
using System.IO;
using System.Linq;
using System.Threading;
using AITestProject.AI.Data;
using Alturos.Yolo;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.VisualBasic.CompilerServices;

namespace AITestProject.AI
{
    public class Training
    {
        private readonly MLContext _mlContext;
        private readonly DirectoryInfo _assets;

        public Training()
        {
            _mlContext = new MLContext(7);
            _assets = new DirectoryInfo(Directory.GetCurrentDirectory() + @"\assets\LFW\");
            Console.WriteLine("Found {0} Images. {1} of them have data.",
                              ImageData.ReadImagesFromFile(_assets.FullName).Count(),
                              ImageData.ReadDataFromFile(Directory.GetCurrentDirectory()).Count());
            Start();
        }

        private void Start()
        {
            foreach (var image in ImageData.ReadDataFromFile(Directory.GetCurrentDirectory()))
            {
                Console.WriteLine(
                    $"{Path.GetFileName(image.ImagePath)}:\t" +
                    $"Left Eye: {string.Join(", ", image.LeftEyeCoordinate)}\t" +
                    $"Right Eye: {string.Join(", ", image.RightEyeCoordinate)}"
                );
            }
        }
    }
}