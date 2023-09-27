namespace GeorgeChew.UnityAssessment.EventMessaging
{
    /// <summary>
    /// Event registry, allows for easy access for all the events for each module.
    /// </summary>
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