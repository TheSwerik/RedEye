using Microsoft.ML.Data;

namespace AITestProject.AI.Data
{
    public class ImagePrediction
    {
        [ColumnName("leftEyeArea")] public int[] LeftEyeArea;
        [ColumnName("rightEyeArea")] public int[] RightEyeArea;
    }
}