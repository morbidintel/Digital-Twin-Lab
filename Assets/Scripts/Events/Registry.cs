namespace GeorgeChew.HiverlabAssessment.EventMessaging
{
    public static class Registry
    {
        public static class CityJson
        {
            public static Event OnLoadedAllFiles = new();
            public static Event OnLoadedVertices = new();
            public static Event OnLoadedAllHdbBlocks = new();
        }

        public static class Heatmap
        {
            public static Event OnClickHdbBlock = new();
        }

        public static class UI
        {
            public static Event OnAnyLeftPanelTabToggle = new();
        }
    }
}