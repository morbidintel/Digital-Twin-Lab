using CityGML2GO;
using Gamelogic.Extensions;
using GeorgeChew.HiverlabAssessment.CityJson;
using GeorgeChew.HiverlabAssessment.Heatmap;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace GeorgeChew.HiverlabAssessment.CityJSON
{
    using Events = EventMessaging.Registry.CityJson;

    public class BuildingsGenerator : MonoBehaviour
    {
        [Header("Scene")]
        [SerializeField] private HdbBlockObject buildingPrefab;
        [SerializeField] private Transform buildingsParent;

        [Header("Config")]
        [SerializeField] private int buildingsToLoadPerFrame = 10;
        [SerializeField] private bool loadNonResidential = false;

        // data read from file
        private List<CityObject> cityObjects = null;
        private List<Vector3> vertices = null;

        // generated buildings
        private List<HdbBlockObject> blocks = new();

        private void Awake()
        {
            Assert.IsNotNull(buildingPrefab);

            Events.OnLoadedAllFiles += obj => cityObjects = obj as List<CityObject>;
            Events.OnLoadedVertices += obj => vertices = obj as List<Vector3>;
        }

        private void Start()
        {
            StartCoroutine(LoadBuildings());
        }

        private IEnumerator LoadBuildings()
        {
            yield return new WaitUntil(() => cityObjects != null && vertices != null);

            var sw = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < cityObjects.Count; i++)
            {
                GenerateBuilding(cityObjects[i]);

                if (i % buildingsToLoadPerFrame == 0)
                {
                    yield return new WaitForEndOfFrame();
                }
            }


            Events.OnLoadedAllHdbBlocks.Publish(blocks);

            Debug.Log($"[BuildingsGenerator] " +
                $"Generated {cityObjects.Count} buildings in {sw.ElapsedMilliseconds} ms.");
        }

        private void GenerateBuilding(CityObject cityObject)
        {
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

            GameObject building = hdbBlock.gameObject;
            building.name = cityObject.Address;
            building.transform.localPosition = Vector3.zero;
            building.transform.localScale = Vector3.one;

            Mesh mesh = GenerateMesh(cityObject);
            RecenterBuilding(building, mesh);
            AddMeshComponents(building, mesh);
        }

        private Mesh GenerateMesh(CityObject cityObject)
        {
            List<Mesh> meshes = new();

            if (cityObject == null ||
                cityObject.geometry.Length == 0 ||
                cityObject.geometry[0].boundaries.Length == 0)
            {
                Debug.Log($"[BuildingsGenerator] {cityObject.Address}");
                return null;
            }

            // our dataset has only 1 geometry and 1 boundary per building
            foreach (var verts in cityObject.geometry[0].boundaries[0])
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

            return mainMesh;
        }

        // Create a mesh from the indices indicated in the town geometry,
        // which references the vertices file
        private Mesh CreateMeshFromIndices(int[] indices)
        {
            var coordinates = indices
                // select from vertices list based in index
                .Select(i => vertices[i]);

            // don't draw faces that are facing down, i.e. faces that have all y-coord as 0
            // saves about 18% vertices count
            if (coordinates.All(c => c.y == 0))
            {
                return null;
            }

            return Poly2Mesh.CreateMesh(new() { outside = coordinates.ToList() });
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