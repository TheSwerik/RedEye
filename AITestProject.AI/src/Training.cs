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
            Console.WriteLine("Found {0} Images.", ImageData.ReadFromFile(_assets.FullName).Count());
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
            foreach (var image in ImageData.ReadFromFile(_assets.FullName))
            {
                Console.WriteLine(Environment.NewLine + Path.GetFileName(image.ImagePath));
                foreach (var item in yolo.Detect(image.ImagePath))
                {
                    Console.WriteLine(item.Type);
                }

                Console.ReadLine();
            }
        }

        private static ITransformer Train(MLContext mlContext, string trainDataPath)
        {
            var pipelineForTripTime = mlContext.Transforms.CopyColumns("Label", "LeftEyeArea")
                                               .Append(mlContext.Regression.Trainers.FastTree())
                                               .Append(mlContext.Transforms.CopyColumns(outputcolumn: "tripTime", inputcolumn: "Score"));

            var pipelineForFareAmount = mlContext.Transforms.CopyColumns("Label", "FareAmount")
                                                 .Append(mlContext.Transforms.Categorical.OneHotEncoding("VendorId"))
                                                 .Append(mlContext.Transforms.Categorical.OneHotEncoding("RateCode"))
                                                 .Append(
                                                     mlContext.Transforms.Categorical.OneHotEncoding("PaymentType"))
                                                 .Append(mlContext.Transforms.Concatenate("Features", "VendorId",
                                                                                          "RateCode", "PassengerCount",
                                                                                          "TripDistance",
                                                                                          "PaymentType"))
                                                 .Append(mlContext.Regression.Trainers.FastTree())
                                                 .Append(mlContext.Transforms.CopyColumns(outputcolumn: "fareAmount",
                                                                                          inputcolumn: "Score"));


            var model = pipelineForTripTime.Append(pipelineForFareAmount).Fit(dataView);
            SaveModelAsFile(mlContext, model);
            return model;
        }
    }
}