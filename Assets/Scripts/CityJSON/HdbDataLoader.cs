using GeorgeChew.UnityAssessment.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace GeorgeChew.UnityAssessment.CityJson
{
    using Events = EventMessaging.Registry.CityJson;

    [Serializable]
    public class Geometry
    {
        public int[][][][] boundaries;
    }

    [Serializable]
    public record Attributes
    {
        public string hdb_blk_no;
        public string hdb_street;
        public int hdb_max_floor_lvl;
        public string hdb_residential;
        public string hdb_bldg_contract_town;
        public int hdb_total_dwelling_units;
        public float height;
        public string postcode;
        public string housename;
    }

    [Serializable]
    public record CityObject
    {
        public string type;
        public Attributes attributes;
        public Geometry[] geometry;

        public string Address => $"{attributes.hdb_blk_no} {attributes.hdb_street}";
        public bool isResidential => attributes.hdb_residential == "Y";
    }

    [Serializable]
    public record HdbTownFile
    {
        public Dictionary<string, CityObject> CityObjects = new();
    }

    /// <summary>
    /// Load the HDB and vertex data for all the buildings in the specified directory that
    /// contains JSON files. <br></br>
    /// The number specified in the boundaries array directly correlates
    /// to the index of the coordinates data retrieved from <see cref="VerticesDataLoader"/>.
    /// </summary>
    public class HdbDataLoader : MonoBehaviour
    {
        [SerializeField] private string directory;

        private ConcurrentBag<CityObject> cityObjects = new();

        private void Awake()
        {
            Assert.IsFalse(string.IsNullOrEmpty(directory));
        }

        private void Start()
        {
            directory = Path.Combine(Application.streamingAssetsPath, directory);
            _ = GetFilesInDirectory(directory);
        }

        private async Task GetFilesInDirectory(string directory)
        {
            await Task.Yield(); // force this method to be asynchronous

            var sw = System.Diagnostics.Stopwatch.StartNew();

            // get all the JSON files in the directory and read each in its separate thread
            var tasks = Directory.EnumerateFiles(directory, "*.json")
                .Select(file => ReadFile(file));

            await Task.WhenAll(tasks);

            Functions.Log($"Read {tasks.Count()} files in {sw.ElapsedMilliseconds} ms.");

            Events.OnLoadedHdbData.Publish(cityObjects.ToList());
        }

        private async Task ReadFile(string path)
        {
            var json = await File.ReadAllTextAsync(path);
            var data = JsonConvert.DeserializeObject<HdbTownFile>(json);

            foreach (var cityObject in data.CityObjects.Values)
            {
                cityObjects.Add(cityObject);
            }
        }
    }
}