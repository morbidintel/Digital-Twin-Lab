using Mapbox.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
using UnityEngine.Assertions;
using UnityEngine;

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
            StartCoroutine(GetFilesInDirectory(directory));
        }

        private IEnumerator GetFilesInDirectory(string directory)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            // get all the JSON files in the directory and read each in its separate thread
            List<Task> tasks = new();
            foreach (var file in Directory.EnumerateFiles(directory, "*.json"))
            {
                Task task = Task.Run(() => ReadFile(file));
                tasks.Add(task);
            }

            yield return new WaitUntil(() => tasks.TrueForAll(t => t.IsCompleted));

            Debug.Log($"[HdbDataLoader] " +
                $"Read {tasks.Count} files in {sw.ElapsedMilliseconds} ms.");

            Events.OnLoadedHdbData.Publish(cityObjects.ToList());
        }

        private void ReadFile(string path)
        {
            var json = File.ReadAllText(path);
            var data = JsonConvert.DeserializeObject<HdbTownFile>(json);

            foreach (var cityObject in data.CityObjects.Values)
            {
                cityObjects.Add(cityObject);
            }
        }
    }
}