using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Gamelogic.Extensions;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Debug = UnityEngine.Debug;
using Material = UnityEngine.Material;

namespace CityGML2GO
{
    using GmlHandlers;

    public partial class CityGml2GO : MonoBehaviour
    {
        //[LabelOverride("File-/Directory Name")]
        public string Filename;
        public string SavedMeshesPath = "Assets/Saved Meshes";
        public bool StreamingAssets;
        public Material DefaultMaterial;
        public GameObject Parent;
        //[LabelOverride("Apply automatic or manual translation")]
        public bool ApplyTranslation;
        [HideInInspector]
        public Vector3 ActualTranslate;
        //[LabelOverride("Manual translation (Set to 0,0,0 for automatic)")]
        public Vector3 Translate = Vector3.zero;
        public bool ShowDebug;
        public int MaxConcurrentFilesToLoad;
        public bool ShowCurves;
        public bool Semantics;
        public float CurveThickness;
        public GameObject LineRendererPrefab;
        public bool GenerateColliders;
        public bool MergeMeshes;
        public bool SaveMeshes;
        public bool RecenterMeshes;
        public List<string> SemanticSurfaces = new List<string> { "GroundSurface", "WallSurface", "RoofSurface", "ClosureSurface", "CeilingSurface", "InteriorWallSurface", "FloorSurface", "OuterCeilingSurface", "OuterFloorSurface", "Door", "Window" };

        public Dictionary<string, Coroutine> coroutines = new Dictionary<string, Coroutine>();

        [HideInInspector]
        public SemanticSurfaceMaterial SemanticSurfMat;

        void Start()
        {
            SemanticSurfMat = GetComponent<SemanticSurfaceMaterial>();

            var fn = "";
            if (StreamingAssets)
                fn = Path.Combine(Application.streamingAssetsPath, Filename);
            else
                fn = Filename;

            var attributes = File.GetAttributes(fn);
            if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                SetTranslate(new DirectoryInfo(fn));
                StartCoroutine(RunDirectory(fn));
            }
            else
            {
                SetTranslate(new FileInfo(fn));
                StartCoroutine(Run(fn));
            }
        }

        void OnDestroy()
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);
            System.GC.Collect();
        }

        /// <summary>
        /// As the values of GML are way outside of unitys range, you should apply a global translate vector to it.
        /// SetTranslate tries to calculate that vector.
        /// </summary>
        /// <param name="file"></param>
        void SetTranslate(FileInfo file)
        {
            if (!ApplyTranslation)
            {
                ActualTranslate = Vector3.zero;
                return;
            }

            ActualTranslate = Translate == Vector3.zero ? TranslateVector.GetTranslateVectorFromFile(file) : Translate;
        }

        /// <summary>
        ///         /// As the values of GML are way outside of unitys range, you should apply a global translate vector to it.
        /// SetTranslate tries to calculate that vector.
        /// </summary>
        /// <param name="directory"></param>
        void SetTranslate(DirectoryInfo directory)
        {
            if (!ApplyTranslation)
            {
                ActualTranslate = Vector3.zero;
                return;
            }

            if (Translate != Vector3.zero)
            {
                ActualTranslate = Translate;
                return;
            }

            Vector3 translate = Vector3.zero;
            var count = 0;
            foreach (var fileInfo in directory.GetFiles("*.gml"))
            {
                count++;
                translate += TranslateVector.GetTranslateVectorFromFile(fileInfo);
            }

            ActualTranslate = translate / count;
        }

        /// <summary>
        /// Proccesses all GML files ina directory
        /// </summary>
        /// <param name="directoryName"></param>
        /// <returns></returns>
        IEnumerator RunDirectory(string directoryName)
        {
            float t = Time.realtimeSinceStartup;

            foreach (var gml in Directory.GetFiles(directoryName, "*.gml", SearchOption.AllDirectories))
            {
                var fullpath = Path.Combine(directoryName, gml);
                if (MaxConcurrentFilesToLoad > 0 && coroutines.Count >= MaxConcurrentFilesToLoad)
                    yield return new WaitUntil(() => coroutines.Count < MaxConcurrentFilesToLoad);
                coroutines.Add(fullpath, StartCoroutine(Run(fullpath)));
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitUntil(() => coroutines.Count == 0);
            StopAllCoroutines();

            Debug.Log($"Loaded all files in {directoryName} in {Time.realtimeSinceStartup - t} seconds");
        }

        /// <summary>
        /// Processes a single file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        IEnumerator Run(string fileName)
        {
            string gmlName = "";
            GameObject building = null;
            var oriPoly = new List<Poly2Mesh.Polygon>();
            var Polygons = new Dictionary<string, GameObject>();
            var Materials = new Dictionary<string, List<string>>();
            var Textures = new List<TextureInformation>();

            try
            {
                using (XmlReader reader = XmlReader.Create(fileName, new XmlReaderSettings { IgnoreWhitespace = true }))
                {
                    while (!reader.EOF)
                    {
                        reader.Read();
                        if (reader.LocalName == "CityModel")
                        {
                            break;
                        }
                    }

                    var version = 0;
                    for (int i = 0; i < reader.AttributeCount; i++)
                    {
                        var attr = reader.GetAttribute(i);
                        if (attr == "http://www.opengis.net/citygml/1.0")
                        {
                            version = 1;
                            break;
                        }
                        if (attr == "http://www.opengis.net/citygml/2.0")
                        {
                            version = 2;
                            break;
                        }
                    }

                    if (version == 0)
                    {
                        Debug.LogWarning("Possibly invalid xml. Check for xml:ns citygml version.");
                    }

                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            if (reader.LocalName == "gml:name")
                            {
                                gmlName = reader.ReadInnerXml();
                            }
                            else if (reader.LocalName == "cityObjectMember")
                            {
                                while (reader.Read())
                                {
                                    if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "Building")
                                    {
                                        building = BuildingHandler.HandleBuilding(reader, this, Textures, Polygons, Materials, oriPoly);
                                        building.name = Path.GetFileNameWithoutExtension(fileName);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.Log($"Unable to load {fileName}\n{e.Message}");
                coroutines.Remove(fileName);
                yield break;
            }

            yield return MaterialHandler.ApplyMaterials(Textures, Polygons, oriPoly, fileName);
            if (MergeMeshes) MergeBuildingMeshes(building);
            if (SaveMeshes) SaveBuildingMeshes(building);

            building.SetActive(true);

            coroutines.Remove(fileName);
        }

        void MergeBuildingMeshes(GameObject building)
        {
            var polygons = building.transform
                .GetComponentsInChildren<MeshFilter>()
                .Where(x => x != null);

            if (polygons.Count() == 0) return;

            var mf = building.AddComponent<MeshFilter>();
            var mr = building.AddComponent<MeshRenderer>();
            var combines = new List<CombineInstance>();

            foreach (var poly in polygons)
            {
                var combine = new CombineInstance();
                combine.mesh = poly.sharedMesh;
                combine.transform = poly.transform.localToWorldMatrix;
                combines.Add(combine);
            }

            mf.mesh = new Mesh();
            mf.mesh.CombineMeshes(combines.ToArray(), true);

            if (GenerateColliders)
                building.AddComponent<MeshCollider>().sharedMesh = mf.mesh;

            // recenter mesh
            if (RecenterMeshes)
            {
                var bounds = mf.mesh.bounds;
                var vertices = mf.mesh.vertices;
                // move vertices to origin and ensure that all buildings are level
                Vector3 newPos = Epsg3414Conversion.DistanceFromCenter(-ActualTranslate + bounds.center).WithY(0);
                Vector3 vertTranslate = bounds.center.WithY(vertices.Min(v => v.y));
                for (int i = 0; i < vertices.Length; ++i)
                    vertices[i] -= vertTranslate;
                mf.mesh.vertices = vertices;
                mf.mesh.RecalculateNormals();
                mf.mesh.RecalculateBounds();
                building.transform.position = newPos;
            }

            mr.materials = polygons.First().GetComponent<MeshRenderer>().materials;

            foreach (var child in polygons)
                Destroy(child.gameObject);
        }

        void SaveBuildingMeshes(GameObject building)
        {
#if UNITY_EDITOR
            var mf = building.GetComponent<MeshFilter>();
            var mr = building.GetComponent<MeshRenderer>();
            if (!mf || !mr) return;
            string assetSavePath = Path.Combine(SavedMeshesPath, building.name + ".asset");

            Mesh mesh = Instantiate(mf.mesh);
            mesh.name = $"{building.name}_Mesh";
            MeshUtility.Optimize(mesh);

            Texture texture = Instantiate(mr.material.mainTexture);
            texture.name = $"{building.name}_Texture";

            Material material = Instantiate(mr.material);
            material.name = $"{building.name}_Material";
            material.mainTexture = texture;

            BuildingScriptableObject bso = new BuildingScriptableObject();
            bso.unityWorldPosition = building.transform.position;
            bso.mesh = mesh;
            bso.texture = texture;
            bso.material = material;

            AssetDatabase.CreateAsset(bso, assetSavePath);
            AssetDatabase.AddObjectToAsset(mesh, assetSavePath);
            AssetDatabase.AddObjectToAsset(material, assetSavePath);
            AssetDatabase.AddObjectToAsset(texture, assetSavePath);
            AssetDatabase.SaveAssets();
#endif
        }

        public bool BuildingHasSavedAsset(string assetName)
        {
            string assetSavePath = Path.Combine(SavedMeshesPath, assetName + ".asset");
            return File.Exists(assetSavePath);
        }

        public static GameObject LoadBuildingFromAssets(string path)
        {
            path = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
            path = path.Replace("Assets\\", "").Replace("Resources\\", "").Replace("\\", "/");
            var bso = Resources.Load<BuildingScriptableObject>(path);
            //var bso = AssetDatabase.LoadAssetAtPath<BuildingScriptableObject>(path);
            //var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            //var texture = AssetDatabase.LoadAssetAtPath<Texture>(path);
            //var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            //material.mainTexture = texture;

            var go = new GameObject();
            go.transform.position = bso.unityWorldPosition;
            go.AddComponent<MeshFilter>().mesh = bso.mesh;
            go.AddComponent<MeshRenderer>().material = bso.material;
            go.name = Path.GetFileNameWithoutExtension(path);
            return go;
        }

        public static IEnumerator LoadBuildingFromAssetsAsync(string path, System.Action<GameObject> onCreate = null)
        {
            path = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path))
                .Replace("Assets\\", "").Replace("Resources\\", "").Replace("\\", "/");
            var req = Resources.LoadAsync<BuildingScriptableObject>(path);

            yield return new WaitUntil(() => req.isDone);

            var bso = req.asset as BuildingScriptableObject;
            var go = new GameObject();
            go.transform.position = bso.unityWorldPosition;
            go.AddComponent<MeshFilter>().mesh = bso.mesh;
            go.AddComponent<MeshRenderer>().material = bso.material;
            go.name = Path.GetFileNameWithoutExtension(path);

            onCreate?.Invoke(go);
            yield break;
        }
    }
}
