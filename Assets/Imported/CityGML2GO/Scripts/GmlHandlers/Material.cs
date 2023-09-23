using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;

namespace CityGML2GO.GmlHandlers
{
    using static CityGml2GO;

    public class MaterialHandler
    {
        public static void HandleMaterial(XmlReader reader, Dictionary<string, List<string>> Materials)
        {
            var id = "";
            while (reader.MoveToNextAttribute())
            {
                if (reader.LocalName == "id")
                {
                    id = reader.Value;
                }
            }
            var targets = new List<string>();
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "target")
                {
                    targets.Add(reader.ReadInnerXml());
                }
                if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "X3DMaterial")
                {
                    break;
                }
            }
            if (!Materials.ContainsKey(id))
                Materials.Add(id, targets);
        }

        public static IEnumerator ApplyMaterials(
            List<TextureInformation> Textures,
            Dictionary<string, GameObject> Polygons,
            List<Poly2Mesh.Polygon> oriPoly,
            string filename = null)
        {
            string path = Path.GetDirectoryName(filename);
            var loadedTextures = new Dictionary<string, Texture2D>();

            foreach (var texture in Textures)
            {
                var tex2D = new Texture2D(1, 1);
                var fn = Path.Combine(path, texture.Url)
                    .Replace(".tif", ".jpg")
                    .Replace("./", "");

                if (loadedTextures.ContainsKey(fn))
                {
                    tex2D = loadedTextures[fn];
                }
                else
                {
                    using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file://" + fn))
                    {
                        yield return uwr.SendWebRequest();
                        if (uwr.isNetworkError || uwr.isHttpError) Debug.Log(uwr.error);
                        else tex2D = DownloadHandlerTexture.GetContent(uwr);
                        tex2D.name = fn;
                        loadedTextures[fn] = tex2D;
                    }
                }

                var mat = new Material(Shader.Find("Standard")) { mainTexture = tex2D };
                mat.name = texture.Url;

                foreach (var textureTarget in texture.Targets)
                {
                    try
                    {
                        GameObject go = Polygons[textureTarget.Id];
                        if (go == null || !go.activeSelf) continue;

                        MeshRenderer mr = go.GetComponent<MeshRenderer>();
                        mr.material = mat;
                        mr.receiveShadows = false;
                        MeshFilter mf = go.GetComponent<MeshFilter>();
                        Vector3[] vertices = mf.sharedMesh.vertices;

                        var uv = new Vector2[vertices.Length];
                        foreach (var x in oriPoly)
                        {
                            if (x.name == textureTarget.Id)
                            {
                                x.outsideUVs = textureTarget.Coords;
                                for (int i = 0; i < vertices.Length; i++)
                                {
                                    uv[i] = x.ClosestUV(vertices[i]);
                                }
                            }
                        }
                        uv.Reverse();
                        mf.sharedMesh.uv = uv.ToArray();
                        mf.sharedMesh.RecalculateTangents();
                    }
                    catch (Exception e)
                    {
                        Debug.Log($"Unable to load {textureTarget.Id} on {texture.Url.Replace(".tif", ".jpg")}.\n{e.Message}\n{e.StackTrace}");
                    }
                }
            }
        }
    }
}
