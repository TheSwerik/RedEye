using System;
using System.Collections.Generic;
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
            Train();
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

        private ITransformer Train()
        {
            var data = _mlContext.Data.LoadFromTextFile<ImageData>("assets/ImageData.tsv", ',');
            var splitData = _mlContext.Data.TrainTestSplit(data, 0.2);
            var testData = splitData.TestSet;
            var trainData = splitData.TrainSet;

            var dataPrepEstimator =
                    _mlContext.Transforms
                              .Concatenate("Label", "LeftEyeArea")
                              // .Append(_mlContext.Transforms.CopyColumns("Label", "LeftEyeArea"))
                              .Append(_mlContext.Transforms.Categorical.OneHotEncoding("ImagePathEncoded", "ImagePath"))
                              .Append(_mlContext.Transforms.Concatenate("Features", "ImagePathEncoded"))
                ;

            // Create data prep transformer & Apply transforms to training data
            var transformedTrainingData = dataPrepEstimator.Fit(trainData).Transform(trainData);

            var model1 = _mlContext.Regression.Trainers.FastTree().Fit(transformedTrainingData);
            ClassifySingleImage(_mlContext, model1);
            return model1;


            // var pipelineForFareAmount = mlContext.Transforms.CopyColumns("Label", "FareAmount")
            //                                      .Append(mlContext.Transforms.Categorical.OneHotEncoding("VendorId"))
            //                                      .Append(mlContext.Transforms.Categorical.OneHotEncoding("RateCode"))
            //                                      .Append(
            //                                          mlContext.Transforms.Categorical.OneHotEncoding("PaymentType"))
            //                                      .Append(mlContext.Transforms.Concatenate("Features", "VendorId",
            //                                                                               "RateCode", "PassengerCount",
            //                                                                               "TripDistance",
            //                                                                               "PaymentType"))
            //                                      .Append(mlContext.Regression.Trainers.FastTree())
            //                                      .Append(mlContext.Transforms.CopyColumns(
            //                                                  "fareAmount",
            //                                                  "Score"));
            //
            //
            // var model = pipelineForTripTime.Append(pipelineForFareAmount).Fit(data);
            // return model;
        }

        public static void ClassifySingleImage(MLContext mlContext, ITransformer model)
        {
            var imageData = new ImageData
                            {
                                ImagePath = @"assets\LFW\Philippe_Noiret\Philippe_Noiret_0001.jpg"
                            };
            // Make prediction function (input = ImageData, output = ImagePrediction)
            var predictor = mlContext.Model.CreatePredictionEngine<ImageData, ImagePrediction>(model);
            var prediction = predictor.Predict(imageData);
            Console.Write($"Image: {Path.GetFileName(prediction.ImagePath)} predicted: ");
            Console.Write(
                $"Left Eye: {string.Join(", ", prediction.PredictedLeftEyeAreaValue)} with score: {prediction.Score.Max()} ");
            Console.WriteLine(
                $"Right Eye: {string.Join(", ", prediction.PredictedRightEyeAreaValue)} with score: {prediction.Score.Max()} ");
        }

        private static void DisplayResults(IEnumerable<ImagePrediction> imagePredictionData)
        {
            foreach (var prediction in imagePredictionData)
            {
                Console.Write($"Image: {Path.GetFileName(prediction.ImagePath)} predicted: ");
                Console.Write(
                    $"Left Eye: {string.Join(", ", prediction.PredictedLeftEyeAreaValue)} with score: {prediction.Score.Max()} ");
                Console.WriteLine(
                    $"Right Eye: {string.Join(", ", prediction.PredictedRightEyeAreaValue)} with score: {prediction.Score.Max()} ");
            }
        }
    }
}