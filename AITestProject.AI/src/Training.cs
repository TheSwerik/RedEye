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
            Console.WriteLine("Found {0} Images.", ImageData.ReadDataFromFile(_assets.FullName).Count());
            Start();
        }

        private void Start()
        {
            var configurationDetector = new ConfigurationDetector();
            var config = configurationDetector.Detect();
            var yolo = new YoloWrapper(config);
            Detect(yolo);
        }

        private void Detect(YoloWrapper yolo)
        {
            foreach (var image in ImageData.ReadDataFromFile(_assets.FullName))
            {
                Console.WriteLine(Environment.NewLine + Path.GetFileName(image.ImagePath));
                foreach (var item in yolo.Detect(image.ImagePath))
                {
                    Console.WriteLine(item.Type);
                }

                Console.ReadLine();
            }
        }
    }
}