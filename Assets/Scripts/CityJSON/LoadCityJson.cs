using CityGML2GO;
using Gamelogic.Extensions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.Assertions;
using UnityEngine;

namespace GeorgeChew.HiverlabAssessment.CityJSON
{
    using Heatmap;
    using Events = EventMessaging.Registry.CityJson;
    using static CityJSON_HDB;

    /// <summary>
    /// Loads CityJSON files using CityGML2GO
    /// </summary>
    public class LoadCityJson : MonoBehaviour
    {
        [SerializeField]
        private string verticesFilePath;

        [SerializeField]
        private string townJsonsPath;

        [SerializeField]
        private GameObject buildingPrefab;

        // allows for the filtering of non-residential blocks in the loading process
        [SerializeField]
        private bool loadNonResidential = false;

        // limit the number of buildings to be loaded per frame
        [SerializeField]
        private int BuildingsToLoadPerFrame;

        [ReadOnly]
        public List<HdbBlockObject> blocks = new();

        // vertices containing coordinates are split to another file in CSV format
        private List<Vector3> vertices = new();

        // for coroutines to wait for vertices to load
        private bool hasLoadedVertices = false;

        // for coroutines to load one by one, instead of all together
        private bool isLoadingTown = false;

        // used for cancelling Tasks
        private CancellationTokenSource cts = new();

        private Dictionary<string, bool> coroutinesHasLoaded = new();

        private bool HasLoadedAll =>
            coroutinesHasLoaded.Count > 0 &&
            coroutinesHasLoaded.Values.All(b => b);

        // Start is called before the first frame update
        private void Start()
        {
            verticesFilePath = Path.Combine(Application.streamingAssetsPath, verticesFilePath);
            townJsonsPath = Path.Combine(Application.streamingAssetsPath, townJsonsPath);

            // move the loading of the vertices file to another thread
            var task = Task.Factory.StartNew(LoadVerticesFile, cts.Token);

            // get the file attributes for file or directory
            FileAttributes attr = File.GetAttributes(townJsonsPath);
            if (attr.HasFlag(FileAttributes.Directory))
            {
                // load the buildings in each town
                foreach (var file in Directory.EnumerateFiles(townJsonsPath, "*.json"))
                {
                    StartCoroutine(LoadTownCoroutine(file));
                    coroutinesHasLoaded[file] = false;
                }
            }
            else
            {
                if (Path.GetExtension(townJsonsPath) != ".json")
                {
                    Debug.LogError("[LoadCityJson] Specified townJsonsPath is not a Json file");
                }
                else
                {
                    StartCoroutine(LoadTownCoroutine(townJsonsPath));
                    coroutinesHasLoaded[townJsonsPath] = false;
                }
            }

            StartCoroutine(WaitForAllLoaded());
        }

        private IEnumerator WaitForAllLoaded()
        {
            yield return new WaitUntil(() => HasLoadedAll);
            Events.OnLoadedAllHdbBlocks.Publish(blocks);
        }

        private void LoadVerticesFile()
        {
            // this is to check if the task is cancelled from main thread
            if (cts.Token.IsCancellationRequested) return;

            vertices.Clear();
            try
            {
                if (cts.Token.IsCancellationRequested) return;

                // each line in the CSV is a 3D coordinate in EPSG:3414
                var file = File.ReadAllLines(verticesFilePath);

                foreach (var l in file)
                {
                    if (cts.Token.IsCancellationRequested) return;
                    var split = l.Split(',').Select(c => float.Parse(c)).ToArray();
                    vertices.Add(new Vector3(split[0], split[2], split[1]));
                }

                hasLoadedVertices = true;

                Debug.Log($"[LoadCityJson] Loaded {verticesFilePath}");
            }
            catch (System.Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        private void OnDestroy()
        {
            // send a Cancel to any Task that's running
            cts.Cancel();
        }

        private IEnumerator LoadTownCoroutine(string path)
        {
            CityJSON_HDB hdb = null;

            // move the loading of the json file to another thread
            var task = Task.Factory.StartNew(ReadFileInThread, cts.Token);

            string townName = Path.GetFileNameWithoutExtension(path).Replace("hdb_", "");
            GameObject town = new GameObject(townName);
            town.transform.parent = transform;
            // in case the parent object has any positioning and scaling
            town.transform.localPosition = Vector3.zero;
            town.transform.localScale = Vector3.one;

            // wait for the town file to be loaded
            yield return new WaitUntil(() => task.IsCompleted);

            // create parent for street name
            Dictionary<string, Transform> zones = new();
            foreach (var zoneName in hdb.CityObjects.Values
                .Select(c =>
                {
                    string housename = c.attributes["osm_addr:housename"];
                    bool validHousename =
                        c.attributes.ContainsKey("osm_addr:housename") &&
                        !string.IsNullOrEmpty(housename);
                    return validHousename ? housename : c.attributes["hdb_street"];
                })
                //.Select(c => c.attributes["hdb_street"])
                .Distinct())
            {
                var zone = new GameObject(zoneName).transform;
                zone.parent = town.transform;
                zone.localPosition = Vector3.zero;
                zone.localScale = Vector3.one;
                zones[zoneName] = zone;
            }

            // wait for the vertices to load, and for the previous town (if any) to load
            yield return new WaitUntil(() => hasLoadedVertices && !isLoadingTown);

            isLoadingTown = true;

            yield return new WaitForEndOfFrame();

            foreach (var cobj in hdb.CityObjects.Values)
            {
                string isResidential = cobj.attributes["hdb_residential"];
                if (!loadNonResidential && isResidential == "N") continue;

                string zoneName = cobj.attributes["osm_addr:housename"],
                    streetName = cobj.attributes["hdb_street"],
                    buildingName = $"{cobj.attributes["hdb_blk_no"]} {streetName}";

                Transform parent = zoneName != null && zones.ContainsKey(zoneName)
                    ? zones[zoneName] : zones[streetName];
                GameObject building = Instantiate(buildingPrefab, parent);
                building.SetActive(false);
                building.name = buildingName;
                // in case the parent object has any positioning and scaling
                building.transform.localPosition = Vector3.zero;
                building.transform.localScale = Vector3.one;
                var block = RetrieveAttributes(building, cobj);

                CreateBuildingFromCityObject(building, cobj);

                building.SetActive(true);
                blocks.Add(block);

                // limit the number of buildings to be loaded per frame
                if (BuildingsToLoadPerFrame > 0 &&
                    blocks.Count % BuildingsToLoadPerFrame == BuildingsToLoadPerFrame - 1)
                    yield return new WaitForEndOfFrame();
            }

            isLoadingTown = false;
            coroutinesHasLoaded[path] = true;
            Debug.Log($"[LoadCityJson] Done generating buildings for {townName}");

            void ReadFileInThread()
            {
                try
                {
                    if (cts.Token.IsCancellationRequested) return;

                    var json = File.ReadAllText(path);

                    if (cts.Token.IsCancellationRequested) return;

                    hdb = JsonConvert.DeserializeObject<CityJSON_HDB>(json);
                    Debug.Log($"[LoadCityJson] Done loading {path}");
                }
                catch (System.Exception e)
                {
                    Debug.Log(e.Message);
                }
            }
        }

        private HdbBlockObject RetrieveAttributes(GameObject building, CityObject cobj)
        {
            var block = building.GetComponent<HdbBlockObject>();
            if (!block) block = building.AddComponent<HdbBlockObject>();

            var data = block.HdbData;
            var attributes = cobj.attributes;
            data.blk_no = attributes["hdb_blk_no"];
            data.street = attributes["hdb_street"];
            data.address = $"{data.blk_no} {data.street}";
            data.bldg_contract_town = attributes["hdb_bldg_contract_town"];
            //data.division = data.bldg_contract_town;
            //data.precinct = data.street;
            data.total_dwelling_units = int.Parse(attributes["hdb_total_dwelling_units"]);
            data.max_floor_lvl = int.Parse(attributes["hdb_max_floor_lvl"]);
            data.height = double.Parse(attributes["height"]);

            //block.isResidential = attributes["hdb_residential"] == "Y";
            //if (block.isResidential)
            //{
            //	block.hasGroundFloorUnit =
            //		data.total_dwelling_units % data.max_floor_lvl == 0;
            //	block.unitsPerFloor = data.total_dwelling_units /
            //		(data.max_floor_lvl - (block.hasGroundFloorUnit ? 1 : 0));
            //}

            return block;
        }

        // Create mesh from the CityObject vertices and add it to a GameObject
        private void CreateBuildingFromCityObject(GameObject building, CityObject cobj)
        {
            List<Mesh> meshes = new List<Mesh>();

            if (cobj.geometry.Length == 0 || cobj.geometry[0].boundaries.Length == 0)
            {
                Debug.Log($"[LoadCityJson] {building.name} has no geometry");
            }

            // our dataset has only 1 geometry and 1 boundary per building
            foreach (var verts in cobj.geometry[0].boundaries[0])
            {
                foreach (var vert in verts)
                {
                    Mesh mesh = CreateMeshFromIndices(vert);
                    if (mesh) meshes.Add(mesh);
                }
            }

            // combine the meshes
            var combines = meshes
                .Select(m => new CombineInstance() { mesh = m })
                .ToArray();
            Mesh mainMesh = new Mesh();
            mainMesh.CombineMeshes(combines, true, false);
            mainMesh.RecalculateNormals();
            mainMesh.RecalculateBounds();

            // recenter the building
            RecenterBuilding(building, mainMesh);
            AddMeshComponents(building, mainMesh);
        }

        // Same as CreateBuildingFromCityObject() but doesn't merge the faces
        // Used for debugging purposes only
        private void CreateBuildingFromCityObjectNoMerge(GameObject building, CityObject cobj)
        {
            Assert.AreEqual(cobj.geometry.Length, 1);
            Assert.AreEqual(cobj.geometry[0].boundaries.Length, 1);

            foreach (var verts in cobj.geometry[0].boundaries[0])
            {
                foreach (var vert in verts)
                {
                    Mesh mesh = CreateMeshFromIndices(vert);
                    if (!mesh) continue;

                    // reposition the face
                    GameObject face = new GameObject();
                    face.transform.parent = building.transform;
                    RecenterBuilding(face, mesh, false);
                    AddMeshComponents(face, mesh);
                }
            }
        }

        // Create a mesh from the indices indicated in the town geometry,
        // which references the vertices file
        private Mesh CreateMeshFromIndices(long[] indices)
        {
            var coordinates = indices
                // select from vertices list based in index
                .Select(i => vertices[(int)i])
                // given coordinates face inwards, so reverse them
                .Reverse();

            // don't draw faces that are facing down, i.e. faces that have all y-coord as 0
            // saves about 18% vertices count
            if (coordinates.All(c => c.y == 0))
                return null;
            else
                return Poly2Mesh.CreateMesh(new Poly2Mesh.Polygon()
                { outside = coordinates.ToList(), });
        }

        // Move the Building and its mesh so that the mesh are centered at (0,0,0)
        // and the building is moved to the original position of the mesh center
        private void RecenterBuilding(GameObject building, Mesh mesh, bool ignoreY = true)
        {
            // save the position of the mesh bounds
            Vector3 newPos = Epsg3414Conversion.DistanceFromCenter(mesh.bounds.center.WithY(0));

            // Move the vertices so that the mesh center is (0,0,0)
            var temp = mesh.vertices;
            var vertTranslate = mesh.bounds.center;
            if (ignoreY) vertTranslate = vertTranslate.WithY(0);
            for (int i = 0; i < temp.Length; ++i)
                temp[i] -= vertTranslate;
            mesh.vertices = temp;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            // reposition the building
            building.transform.localPosition = newPos;
        }

        private void AddMeshComponents(GameObject go, Mesh mesh)
        {
            var mf = go.GetComponent<MeshFilter>();
            if (!mf) mf = go.AddComponent<MeshFilter>();
            mf.mesh = mesh;

            var mr = go.GetComponent<MeshRenderer>();
            if (!mr) mr = go.AddComponent<MeshRenderer>();
            if (!mr.material) mr.material = new Material(Shader.Find("Standard"));
            mr.receiveShadows = false;
        }
    }
}