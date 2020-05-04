using System;
using System.IO;
using System.Linq;
using AITestProject.AI.Data;
using Microsoft.ML;

namespace AITestProject.AI
{
    public class Training
    {
        private readonly DirectoryInfo _assets;
        private readonly MLContext _mlContext;

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
                Console.WriteLine(
                    $"{Path.GetFileName(image.ImagePath)}:\t" +
                    $"Left Eye: {string.Join(", ", image.LeftEyeArea)}\t" +
                    $"Right Eye: {string.Join(", ", image.RightEyeArea)}"
                );
        }

        private static ITransformer Train(MLContext mlContext, string trainDataPath)
        {
            mlContext = new MLContext();
            var data = mlContext.Data.LoadFromTextFile<ImageData>("assets/ImageData.tsv", ',');
            var splitData = mlContext.Data.TrainTestSplit(data, 0.2);
            var testData = splitData.TestSet;
            var trainData = splitData.TrainSet;

            var dataPrepEstimator = mlContext.Transforms
                                             .Concatenate("Features", "ImagePath", "LeftEyeArea")
                                             .Append(mlContext.Transforms.NormalizeMinMax("Features"));

            // Create data prep transformer & Apply transforms to training data
            var transformedTrainingData = dataPrepEstimator.Fit(trainData).Transform(trainData);


            var pipelineForTripTime = mlContext.Transforms.CopyColumns("Label", "LeftEyeArea")
                                               .Append(mlContext.Regression.Trainers.FastTree())
                                               .Append(mlContext.Transforms.CopyColumns(
                                                           "leftEyeArea",
                                                           "Score"));

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
                                                 .Append(mlContext.Transforms.CopyColumns(
                                                             "fareAmount",
                                                             "Score"));


            var model = pipelineForTripTime.Append(pipelineForFareAmount).Fit(data);
            return model;
        }
    }
}