using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CityGML2GO
{
    using Scripts;

    [CustomEditor(typeof(BuildingProperties))]
    public class BuildingPropertiesEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var sb = new StringBuilder();
            foreach (var o in this.targets)
            {
                var props = (BuildingProperties)o;
                foreach (var buildingProperty in props.Properties)
                {
                    sb.AppendFormat("{0}: {1}\r\n", buildingProperty.Key, buildingProperty.Value);
                }
            }
            GUILayout.Label(sb.ToString().Take(255).ToString());
        }
    }
}