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
                    $"Left Eye: {image.LeftEyeX}, {image.LeftEyeY}\t" +
                    $"Right Eye: {image.RightEyeX}, {image.RightEyeY}"
                );
        }

        private ITransformer Train()
        {
            var data = _mlContext.Data.LoadFromTextFile<ImageData>("assets/ImageData.tsv", ',');
            var splitData = _mlContext.Data.TrainTestSplit(data, 0.2);
            var testData = splitData.TestSet;
            var trainData = splitData.TrainSet;

            var dataPrepEstimator = _mlContext.Transforms
                                              .Concatenate("Features", "LeftEyeX", "LeftEyeY", "RightEyeX", "RightEyeY")
                ;

            var model1 = dataPrepEstimator.Fit(testData);
            ClassifySingleImage(model1);
            return model1;
        }

        public void ClassifySingleImage(ITransformer model)
        {
            var imageData = new ImageData
                            {
                                ImagePath = @"assets\LFW\Philippe_Noiret\Philippe_Noiret_0001.jpg"
                            };
            // Make prediction function (input = ImageData, output = ImagePrediction)
            var predictor = _mlContext.Model.CreatePredictionEngine<ImageData, ImagePrediction>(model);
            var prediction = predictor.Predict(imageData);
            Console.Write($"Image: {Path.GetFileName(prediction.PredictedImageName)} predicted: " +
                          (prediction.Positions == null
                               ? "NULLLLLL"
                               : $"Data: {string.Join(", ", prediction.Positions)}"));
        }

        private static void DisplayResults(IEnumerable<ImagePrediction> imagePredictionData)
        {
            foreach (var prediction in imagePredictionData)
                Console.Write($"Image: {Path.GetFileName(prediction.PredictedImageName)} predicted: " +
                              $"Data: {string.Join(", ", prediction.Positions)}");
        }
    }
}