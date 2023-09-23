using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CityGML2GO
{
	public class LoadBuildingScriptableObject : MonoBehaviour
	{
		[SerializeField]
		string directoryOrFile = "";

		[SerializeField]
		int filesToLoadInPerFrame = 10;

		public List<GameObject> buildings = new List<GameObject>();

		// Start is called before the first frame update
		void Start()
		{
			if (!string.IsNullOrEmpty(directoryOrFile))
				StartCoroutine(LoadBuildingsCoroutine());
		}

		IEnumerator LoadBuildingsCoroutine()
		{
			float t = Time.realtimeSinceStartup;
			// is a directory
			if ((File.GetAttributes(directoryOrFile) & FileAttributes.Directory) == FileAttributes.Directory)
			{
				var files = Directory.EnumerateFiles(directoryOrFile, "*.asset", SearchOption.AllDirectories);
				foreach (var file in files)
				{
					yield return StartCoroutine(
						CityGml2GO.LoadBuildingFromAssetsAsync(file, go =>
						{
							buildings.Add(go);
							go.transform.parent = transform;
						}));
				}

				Debug.Log($"Loaded {buildings.Count} buildings in {directoryOrFile}\n" +
					$"Load time: {Time.realtimeSinceStartup - t}");
			}
			else // is a file
			{
				if (Path.GetExtension(directoryOrFile) != ".asset")
				{
					Debug.Log("Must be a .asset file");
					yield break;
				}
				else
				{
					LoadBuilding(directoryOrFile);
					Debug.Log($"Loaded building {directoryOrFile}\n" +
						$"Load time: {Time.realtimeSinceStartup - t}");
				}
			}
		}

		void LoadBuilding(string filename)
		{
			var building = CityGml2GO.LoadBuildingFromAssets(filename);
			building.transform.parent = transform;
			buildings.Add(building);
		}
	}
}
