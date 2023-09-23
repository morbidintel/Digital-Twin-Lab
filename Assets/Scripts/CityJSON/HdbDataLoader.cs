using Mapbox.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using System;
using UnityEngine.Assertions;
using UnityEngine;
using System.Linq;

namespace GeorgeChew.HiverlabAssessment.CityJson
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

    public class HdbDataLoader : MonoBehaviour
    {
        [SerializeField] private string directory;

        private Dictionary<string, bool> filesToRead = new();
        private ConcurrentBag<CityObject> cityObjects = new();

        private void Awake()
        {
            Assert.IsFalse(string.IsNullOrEmpty(directory));

            directory = Path.Combine(Application.streamingAssetsPath, directory);
            StartCoroutine(GetFilesInDirectory(directory));
        }

        private IEnumerator GetFilesInDirectory(string directory)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            foreach (var file in Directory.EnumerateFiles(directory, "*.json"))
            {
                filesToRead.Add(file, false);
                Task.Run(() => ReadFile(file));
            }

            yield return new WaitUntil(() => filesToRead.Values.All(v => v));

            Debug.Log($"[HdbDataLoader] " +
                $"Read {filesToRead.Count} files in {sw.ElapsedMilliseconds} ms.");

            Events.OnLoadedAllFiles.Publish(cityObjects.ToList());
        }

        private void ReadFile(string path)
        {
            var json = File.ReadAllText(path);
            var data = JsonConvert.DeserializeObject<HdbTownFile>(json);

            foreach (var cityObject in data.CityObjects.Values)
            {
                cityObjects.Add(cityObject);
            }

            filesToRead[path] = true;
        }
    }
}