using System;
using System.Collections.Generic;

namespace GeorgeChew.HiverlabAssessment.CityJSON
{
    [Serializable]
    public class CityJSON_HDB
    {
        public static Dictionary<string, string> TownsLegend = new()
        {
            { "AMK", "Ang Mo Kio" },
            { "BB", "Bukit Batok" },
            { "BD", "Bedok" },
            { "BH", "Bishan" },
            { "BM", "Bukit Merah" },
            { "BP", "Bukit Panjang" },
            { "BT", "Bukit Timah" },
            { "CCK", "Choa Chu Kang" },
            { "CL", "Clementi" },
            { "CT", "Central Area" },
            { "GL", "Geyland" },
            { "HG", "Hougang" },
            { "JE", "Jurong East" },
            { "JW", "Jurong West" },
            { "KWN", "Kallang Whampoa" },
            { "MP", "Marine Parade" },
            { "PG", "Punggol" },
            { "PRC", "Pasir Ris" },
            { "QT", "Queenstown" },
            { "SB", "Sembawang" },
            { "SGN", "Serangoon" },
            { "SK", "Sengkang" },
            { "TAP", "Tampines" },
            { "TG", "Tengah" },
            { "TP", "Toa Payoh" },
            { "WL", "Woodlands" },
            { "YS", "Yishun" },
        };

        [Serializable]
        public class CityObject
        {
            [Serializable]
            public class Geometry
            {
                public string type;
                public long lod;
                public long[][][][] boundaries;
            }

            public string type;
            public Dictionary<string, string> attributes;
            public Geometry[] geometry;
        }

        //[Serializable]
        //public class Metadata
        //{
        //    [Serializable]
        //    public partial class DatasetPointOfContact
        //    {
        //        public string ContactName { get; set; }
        //        public string EmailAddress { get; set; }
        //        public string ContactType { get; set; }
        //        public Uri Website { get; set; }
        //    }

        //    public string datasetTitle { get; set; }
        //    public DateTime datasetReferenceDate { get; set; }
        //    public string geographicLocation { get; set; }
        //    public string referenceSystem { get; set; }
        //    public double[] geographicalExtent { get; set; }
        //    public DatasetPointOfContact datasetPointOfContact { get; set; }
        //    public string metadataStandard { get; set; }
        //    public string metadataStandardVersion { get; set; }
        //}

        //public string type;
        //public string version;
        //public Metadata metadata { get; set; }
        public Dictionary<string, CityObject> CityObjects;
        public double[][] Vertices { get; set; }
    }
}