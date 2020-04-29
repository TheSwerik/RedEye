namespace AITestProject.AI.Parser
{
    class CellDimensions : DimensionsBase
    {
    }

    public class OutputParser
    {
        public const int ROW_COUNT = 13;
        public const int COL_COUNT = 13;
        public const int CHANNEL_COUNT = 125;
        public const int BOXES_PER_CELL = 5;
        public const int BOX_INFO_FEATURE_COUNT = 5;
        public const int CLASS_COUNT = 20;
        public const float CELL_WIDTH = 32;
        public const float CELL_HEIGHT = 32;

        private int channelStride = ROW_COUNT * COL_COUNT;
    }
}