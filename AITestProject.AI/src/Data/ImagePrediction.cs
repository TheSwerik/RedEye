using Microsoft.ML.Data;

namespace AITestProject.AI.Data
{
    public class ImagePrediction
    {
        [ColumnName("leftEyeArea")] public float[] LeftEyeArea;
        [ColumnName("rightEyeArea")] public float[] RightEyeArea;
    }
}