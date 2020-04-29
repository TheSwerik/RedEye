using Microsoft.ML.Data;

namespace AITestProject.AI.Data
{
    public class ImagePrediction
    {
        [ColumnName("leftEyeArea")] public int[] LeftEyeCoordinate;
        [ColumnName("rightEyeArea")] public int[] RightEyeCoordinate;
    }
}