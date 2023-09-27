using CityGML2GO;
using Gamelogic.Extensions;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.Assertions;
using UnityEngine;

namespace GeorgeChew.UnityAssessment.CityJSON
{
    using Heatmap;
    using CityJson;
    using Events = EventMessaging.Registry.CityJson;

    /// <summary>
    /// Functionality to generate all the buildings using the data from 
    /// <see cref="HdbDataLoader"/> and <see cref="VerticesDataLoader"/>.
    /// </summary>
    /// <remarks>
    /// The field <see cref="doGenerateBuildings"/> should be false, and only turned on in 
    /// the Editor if the buildings need to be regenerated. <br></br>
    /// The generated model should be already be saved as FBX using Unity's FBX Exporter package.
    /// </remarks>
    public class BuildingsGenerator : MonoBehaviour
    {
        [SerializeField] private bool doGenerateBuildings = false;

        [Header("Script References")]
        [SerializeField] private HdbDataLoader hdbDataLoader;
        [SerializeField] private VerticesDataLoader verticesDataLoader;

        [Header("Scene and Prefab References")]
        [SerializeField] private Transform buildingsParent;
        [SerializeField] private HdbBlockObject buildingPrefab;

        [Header("Config")]
        [SerializeField] private int buildingsToLoadPerFrame = 10;
        [SerializeField] private bool onlyLoadResidential = true;

        // data read from file
        private List<CityObject> cityObjects = null;
        private List<Vector3> vertices = null;

        // generated buildings
        private List<HdbBlockObject> blocks = new();

        private void Awake()
        {
            Assert.IsNotNull(hdbDataLoader);
            Assert.IsNotNull(verticesDataLoader);

            if (doGenerateBuildings)
            {
                Assert.IsNotNull(buildingsParent);
                Assert.IsNotNull(buildingPrefab);
            }

            Events.OnLoadedHdbData += obj => cityObjects = obj as List<CityObject>;
            Events.OnLoadedVertices += obj => vertices = obj as List<Vector3>;
        }

        private void Start()
        {
            if (doGenerateBuildings)
            {
                StartLoadingBuildings();
            }
            else
            {
                StartCoroutine(PopulateAndPublishBlocks());
            }
        }

        private IEnumerator PopulateAndPublishBlocks()
        {
            // get existing HdbBlockObjects in the scene
            blocks.AddRange(FindObjectsOfType<HdbBlockObject>());

            yield return new WaitUntil(() => cityObjects != null);

            // find and populate data for the existing HdbBlockObjects
            var cityObjectsDict = cityObjects.ToDictionary(c => c.Address);
            foreach (var block in blocks)
            {
                if (!cityObjectsDict.TryGetValue(block.name, out var cityObject))
                {
                    Debug.Log($"[BuildingsGenerator] Can't find data for {block.name}");
                    continue;
                }

                var attributes = cityObject.attributes;
                block.Initialize(new()
                {
                    blk_no = attributes.hdb_blk_no,
                    street = attributes.hdb_street,
                    address = cityObject.Address,
                    bldg_contract_town = attributes.hdb_bldg_contract_town,
                    postal_code = attributes.postcode,
                    total_dwelling_units = attributes.hdb_total_dwelling_units,
                    max_floor_lvl = attributes.hdb_max_floor_lvl,
                    height = attributes.height,
                });

                if (onlyLoadResidential && !cityObject.isResidential)
                {
                    block.gameObject.SetActive(false);
                }
            }

            // kickstart the scripts that depend on Events.OnLoadedAllFiles
            Events.OnLoadedAllHdbBlocks.Publish(blocks);
        }

        [ContextMenu("Start Loading Buildings")]
        private void StartLoadingBuildings()
        {
            hdbDataLoader.enabled = true;
            verticesDataLoader.enabled = true;
            buildingsParent.gameObject.SetActive(true);
            StartCoroutine(LoadBuildings());
        }

        private IEnumerator LoadBuildings()
        {
            yield return new WaitUntil(() => cityObjects != null && vertices != null);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            bool fragmentedLoading = buildingsToLoadPerFrame > 0;

            for (int i = 0; i < cityObjects.Count; i++)
            {
                GenerateBuilding(cityObjects[i]);

                // load only a certain amount of buildings per frame
                if (fragmentedLoading && i % buildingsToLoadPerFrame == 0)
                {
                    yield return new WaitForEndOfFrame();
                }
            }

            //RecenterAllBuildings();
            //BatchMeshes();

            Events.OnLoadedAllHdbBlocks.Publish(blocks);

            Debug.Log($"[BuildingsGenerator] " +
                $"Generated {cityObjects.Count} buildings in {sw.ElapsedMilliseconds} ms.");
        }

        private void GenerateBuilding(CityObject cityObject)
        {
            if (onlyLoadResidential && !cityObject.isResidential)
                return;

            var attributes = cityObject.attributes;
            var hdbBlock = Instantiate(buildingPrefab, buildingsParent);
            hdbBlock.Initialize(new()
            {
                blk_no = attributes.hdb_blk_no,
                street = attributes.hdb_street,
                address = cityObject.Address,
                bldg_contract_town = attributes.hdb_bldg_contract_town,
                postal_code = attributes.postcode,
                total_dwelling_units = attributes.hdb_total_dwelling_units,
                max_floor_lvl = attributes.hdb_max_floor_lvl,
                height = attributes.height,
            });
            blocks.Add(hdbBlock);

            Vector3 buildingPos = GetBuildingCenter(cityObject);

            GameObject building = hdbBlock.gameObject;
            building.name = cityObject.Address;
            building.transform.localPosition = buildingPos;
            building.transform.localScale = Vector3.one;

            Mesh mesh = GenerateMesh(cityObject, buildingPos);
            AddMeshComponents(building, mesh);
        }

        private Vector3 GetBuildingCenter(CityObject cityObject)
        {
            int[][][] boundary = cityObject.geometry[0].boundaries[0];

            List<Vector3> allVertices = boundary
                .SelectMany(i => i.SelectMany(ii => ii))
                .Select(i => vertices[i])
                .ToList();

            return allVertices.Aggregate((a, b) => a += b) / allVertices.Count;
        }

        private Mesh GenerateMesh(CityObject cityObject, Vector3 position)
        {
            List<Mesh> meshes = new();

            if (cityObject == null ||
                cityObject.geometry.Length == 0 ||
                cityObject.geometry[0].boundaries.Length == 0)
            {
                Debug.Log($"[BuildingsGenerator]" +
                    $"Invalid object or geometry: {cityObject.Address}");
                return null;
            }

            // our dataset has only 1 geometry and 1 boundary per building
            foreach (var verts in cityObject.geometry[0].boundaries[0])
            {
                foreach (var vert in verts)
                {
                    Mesh mesh = CreateMeshFromIndices(vert, position);
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

            return mainMesh;
        }

        // create a mesh from the indices indicated in the building's geometry,
        // which is cross-referenced the vertices file.
        private Mesh CreateMeshFromIndices(int[] indices, Vector3 position)
        {
            // cross-reference index to get coordinates from vertices data
            var coordinates = indices
                .Select(i => vertices[i] - position);

            // don't draw faces that are facing down, i.e. faces that have all y-coord as 0
            // saves about 18% vertices count
            if (coordinates.All(c => c.y == 0))
            {
                return null;
            }

            return Poly2Mesh.CreateMesh(new() { outside = coordinates.ToList() });
        }

        // move the Building and its mesh so that the mesh are centered at (0,0,0)
        // and the building is moved to the original position of the mesh center
        private void RecenterBuilding(GameObject building, Mesh mesh, bool ignoreY = true)
        {
            // save the position of the mesh bounds
            //Vector3 newPos = Epsg3414Conversion.DistanceFromCenter(mesh.bounds.center.WithY(0));
            Vector3 newPos = mesh.bounds.center.WithY(0);

            // Move the vertices so that the mesh center is (0,0,0)
            var temp = mesh.vertices;
            var vertTranslate = mesh.bounds.center;

            if (ignoreY)
            {
                vertTranslate = vertTranslate.WithY(0);
            }

            for (int i = 0; i < temp.Length; ++i)
            {
                temp[i] -= vertTranslate;
            }

            mesh.vertices = temp;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            // reposition the building
            building.transform.localPosition = newPos;
        }

        // add the generated mesh to the MeshFilter, with some added checks.
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

        private void RecenterAllBuildings()
        {
            Bounds bounds = new(blocks[0].transform.position, Vector3.zero);
            foreach (var block in blocks.Skip(1))
            {
                bounds.Encapsulate(block.transform.position);
            }

            var center = bounds.center.WithY(0);
            foreach (var block in blocks)
            {
                block.transform.position -= center;
            }
        }

        private void BatchMeshes()
        {
            var gameObjects = blocks.Select(b => b.gameObject).ToArray();
            StaticBatchingUtility.Combine(gameObjects, buildingsParent.gameObject);
        }
    }
}