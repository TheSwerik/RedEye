using System;
using System.IO;
using AITestProject.AI.Data;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.VisualBasic.CompilerServices;

namespace AITestProject.AI
{
    public class Training
    {
        private readonly MLContext _mlContext;

        public Training()
        {
            _mlContext = new MLContext(7);
            // TODO Load Data
            Init();
            var assetsRelativePath = @"../../../assets";
            var assetsPath = GetAbsolutePath(assetsRelativePath);
            var modelFilePath = Path.Combine(assetsPath, "Model", "TinyYolo2_model.onnx");
            var imagesFolder = Path.Combine(assetsPath, "images");
            var outputFolder = Path.Combine(assetsPath, "images", "output");
        }

        private void Init()
        {
            // var pipeline = _mlContext.Transforms.Conversion.MapValueToKey(
                                     // )
                                     // .Append(
                                         // _mlContext.Model.CreatePredictionEngine<ImageData, ImagePrediction>(null)
                                     // );
        }

        public static string GetAbsolutePath(string relativePath)
        {
            var dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            var assemblyFolderPath = (dataRoot.Directory ?? throw new ArgumentException("dataRoot.Directory is null"))
                .FullName;

            var fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }
    }
}