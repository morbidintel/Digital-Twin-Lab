using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace CityGML2GO.GmlHandlers
{
    public class TextureHandler
    {

        public static void HandleTexture(XmlReader reader, List<CityGml2GO.TextureInformation> Textures)
        {
            //return;
            var texture = new CityGml2GO.TextureInformation();
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "target")
                {
                    var textureTarget = new CityGml2GO.TextureTarget();
                    while (reader.MoveToNextAttribute())
                    {
                        if (reader.LocalName == "uri")
                        {
                            textureTarget.Id = reader.Value.Replace("#", "");
                            //textureTarget.Id = reader.Value.Replace("#", "");
                        }
                    }

                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "textureCoordinates")
                        {
                            var coords = reader.ReadInnerXml();
                            var parts = coords.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                            for (int i = 0; i < parts.Length; i += 2)
                            {
                                textureTarget.Coords.Add(new Vector2((float)double.Parse(parts[i]), (float)double.Parse(parts[i + 1])));
                            }
                        }
                        if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "target")
                        {
                            break;
                        }
                    }
                    texture.Targets.Add(textureTarget);
                }
                if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "imageURI")
                {
                    texture.Url = reader.ReadInnerXml();
                }

                if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "ParameterizedTexture")
                {
                    break;
                }
            }

            Textures.Add(texture);
        }
    }
}
