using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace CityGML2GO.GmlHandlers
{
    public class PositionHandler
    {
        static string CleanPos(string pos)
        {
            return pos.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ");
        }

        public static Vector3 GetPos(XmlReader reader, Vector3 translate)
        {
            if (string.IsNullOrEmpty(reader.Value))
            {
                reader.Read();
            }
            var parts = CleanPos(reader.Value).Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            return new Vector3(float.Parse(parts[0]) + translate.x, float.Parse(parts[2]) + translate.y, float.Parse(parts[1]) + translate.z);
        }

        public static List<Vector3> GetPosList(XmlReader reader, Vector3 translate)
        {
            var retVal = new List<Vector3>();
            var parts = CleanPos(reader.Value).Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                parts = CleanPos(reader.ReadInnerXml()).Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            }
            if (parts.Length % 3 != 0)
            {
                Debug.LogWarning("Error in posList (Count % 3 != 0)");
                Debug.Log(((IXmlLineInfo)reader).LineNumber);
                return null;
            }

            for (int i = 0; i < parts.Length; i += 3)
            {
                retVal.Add(new Vector3((float)double.Parse(parts[i]) + translate.x, (float)double.Parse(parts[i + 2]) + translate.y, (float)double.Parse(parts[i + 1]) + translate.z));
            }

            return retVal;
        }
    }
}
