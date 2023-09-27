namespace GeorgeChew.UnityAssessment.EventMessaging
{
    /// <summary>
    /// Event registry, allows for easy access for all the events for each module.
    /// Using the registry reduces the need for cross-assembly referencing in some use-cases, 
    /// which can prevent circular dependencies.
    /// </summary>
    public static class Registry
    {
        public static class CityJson
        {
            public static Event OnLoadedHdbData = new();
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