using GeorgeChew.UnityAssessment.Utils;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace GeorgeChew.UnityAssessment.CityJson
{
    using Events = EventMessaging.Registry.CityJson;

    /// <summary>
    /// Load the vertices coordinates data from the specified vertices CSV file. <br></br>
    /// The CSV file has no header, and each line contains 3 floats corresponding
    /// to an x, y, z coordinate.
    /// </summary>
    public class VerticesDataLoader : MonoBehaviour
    {
        [SerializeField] private string filepath;

        public Vector3[] Vertices { get; private set; } = null;

        private void Awake()
        {
            Assert.IsFalse(string.IsNullOrEmpty(filepath));
        }

        private void Start()
        {
            filepath = Path.Combine(Application.streamingAssetsPath, filepath);
            Task.Run(() => ReadFile(filepath));
        }

        private async Task ReadFile(string path)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var lines = File.ReadAllLines(path);
            int count = lines.Length;
            Vertices = new Vector3[count];

            var tasks = Enumerable.Range(0, count)
                .Select(i => ReadLine(lines, i));

            await Task.WhenAll(tasks);

            Functions.Log($"Read {Vertices.Length} vertices in {sw.ElapsedMilliseconds} ms.");

            Events.OnLoadedVertices.Publish(Vertices.ToList());
        }

        private async Task ReadLine(string[] lines, int index)
        {
            var line = lines[index];
            var split = line.Split(',');

            if (float.TryParse(split[0], out var x) &&
                float.TryParse(split[1], out var y) &&
                float.TryParse(split[2], out var z))
            {
                Vertices[index] = new(x, y, z);
            }
        }
    }
}