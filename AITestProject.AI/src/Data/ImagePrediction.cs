using Microsoft.ML.Data;

namespace AITestProject.AI.Data
{
    public class ImagePrediction
    {
        [ColumnName("Score")] public float[] Positions;
        [ColumnName("PredictedLabel")] public string PredictedImageName;

        // public float[] PredictedLeftEyeAreaValue;
        // public float[] PredictedRightEyeAreaValue;
        // public float[] Score;
    }
}