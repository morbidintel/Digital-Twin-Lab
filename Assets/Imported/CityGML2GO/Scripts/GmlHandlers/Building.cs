using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CityGML2GO.GmlHandlers
{
	public class BuildingHandler
	{
		public static GameObject HandleBuilding(
			XmlReader reader,
			CityGml2GO cityGml2Go,
			List<CityGml2GO.TextureInformation> Textures,
			Dictionary<string, GameObject> Polygons,
			Dictionary<string, List<string>> Materials,
			List<Poly2Mesh.Polygon> oriPoly)
		{
			var buildingName = "";

			while (reader.MoveToNextAttribute())
			{
				if (reader.LocalName == "id")
				{
					buildingName = reader.Value;
				}
			}

			var buildingGo = new GameObject(string.IsNullOrEmpty(buildingName) ? "Building" : buildingName);
			buildingGo.SetActive(false);

			var buildingProperties = buildingGo.AddComponent<Scripts.BuildingProperties>();
			var semanticType = buildingGo.AddComponent<SemanticType>();
			buildingGo.transform.SetParent(cityGml2Go.Parent.transform);

			while (reader.Read())
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					if (reader.LocalName == "X3DMaterial")
					{
						MaterialHandler.HandleMaterial(reader, Materials);
					}
					else if (reader.LocalName == "ParameterizedTexture")
					{
						TextureHandler.HandleTexture(reader, Textures);
					}
					else if (reader.LocalName == "Polygon")
					{
						var polyGo = PolygonHandler.PolyToMeshGO(reader, buildingName, cityGml2Go, oriPoly, Polygons, semanticType);
						if (polyGo != null)
							polyGo.transform.SetParent(buildingGo.transform);
					}
					else if (cityGml2Go.ShowCurves && reader.LocalName == "MultiCurve")
					{
						MultiCurveHandler.HandleMultiCurve(reader, buildingGo, semanticType, cityGml2Go);
					}
					else if (cityGml2Go.Semantics && cityGml2Go.SemanticSurfaces.Any(x => x == reader.LocalName))
					{
						semanticType.Name = reader.LocalName;
						reader.MoveToFirstAttribute();
						if (reader.LocalName == "id")
						{
							semanticType.Id = reader.Value;
						}
						else
						{
							while (reader.MoveToNextAttribute())
							{
								semanticType.Id = reader.Value;
							}
						}
					}
					//else if (reader.Name == "gml:name")
					//{
					//	var val = reader.ReadInnerXml();
					//	buildingGo.name = !string.IsNullOrEmpty(val) ? val : buildingGo.name;
					//}
					//else if (cityGml2Go.RecenterMeshes && reader.Name == "gml:pos")
					//{
					//	// position the building according to the coord in the file
					//	float[] coord = reader.ReadInnerXml().Split(' ')
					//		.Select(v => float.Parse(v))
					//		.ToArray();
					//	Vector3 coordInEpsg3414 = new Vector3(coord[0], 0, coord[1]);
					//	buildingGo.transform.position = Epsg3414Conversion.DistanceFromCenter(coordInEpsg3414);
					//}
				}

				BuildingPropertiesHandler.HandleBuildingProperties(reader, buildingProperties);

				if (reader.NodeType == XmlNodeType.EndElement &&
					reader.LocalName == "Building")
					break;
			}

			Object.Destroy(semanticType);

			return buildingGo;
		}
	}
}
