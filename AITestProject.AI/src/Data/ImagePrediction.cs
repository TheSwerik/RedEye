using Microsoft.ML.Data;

namespace AITestProject.AI.Data
{
    public class ImagePrediction : ImageData
    {
        // public float[] Score;

        [ColumnName("grid")] public float[] PredictedLabels;
    }
}