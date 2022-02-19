using Chipper.Prefabs.Data;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Chipper.Prefabs.Parser
{
    public static class TextureParser 
    {
        public static List<Sprite> ParseSpriteMetaData(string path)
        {
            var parsingSprites = false;
            var parsingRect = false;
            var parsingRectDepth = 0;
            var parsingSpriteDepth = 0;
            var sprites = new List<Sprite>();

            foreach (var line in File.ReadLines(path))
            {
                var cleanLine = line.TrimStart().ToLower();
                var depth = line.Length - cleanLine.Length;
                var activeSprite = sprites.Count > 0 ? sprites[sprites.Count - 1] : null;

                if (parsingRect && parsingRectDepth == depth)
                    parsingRect = false;

                if (parsingSprites && depth == parsingSpriteDepth && !cleanLine.StartsWith("-"))
                    parsingSprites = false;

                if (parsingRect)
                {
                    int.TryParse(cleanLine.Split(' ')[1], out var value);
                    switch (cleanLine)
                    {
                        case string a when a.StartsWith("x"):
                            activeSprite.Rect.X = value;
                            break;
                        case string a when a.StartsWith("y"):
                            activeSprite.Rect.Y = value;
                            break;
                        case string a when a.StartsWith("width"):
                            activeSprite.Rect.Width = value;
                            break;
                        case string a when a.StartsWith("height"):
                            activeSprite.Rect.Height = value;
                            break;
                    }
                }

                if (parsingSprites)
                {
                    switch (cleanLine)
                    {
                        case string a when a.StartsWith("-"):
                        {
                            sprites.Add(new Sprite());
                        }
                        break;
                        case string a when a.StartsWith("name"):
                        {
                            activeSprite.Name = string.Join(" ", cleanLine.Split(' ').Skip(1));
                        }
                        break;
                        case string a when a.StartsWith("rect"):
                        {
                            activeSprite.Rect = new Rect();
                            parsingRect = true;
                            parsingRectDepth = depth;
                        }
                        break;
                        case string a when a.StartsWith("alignment"):
                        {
                            int.TryParse(cleanLine.Split(' ')[1], out var value);
                            activeSprite.Alignment = value;
                        }
                        break;
                        case string a when a.StartsWith("pivot"):
                        {
                            var spaceIndex = cleanLine.IndexOf(' ');
                            var pivot = JsonConvert.DeserializeObject<Vec2>(cleanLine.Substring(spaceIndex, cleanLine.Length - spaceIndex));
                            activeSprite.Pivot = pivot;
                        }
                        break;
                        case string a when a.StartsWith("border"):
                        {
                            var spaceIndex = cleanLine.IndexOf(' ');
                            var border = JsonConvert.DeserializeObject<Vec4>(cleanLine.Substring(spaceIndex, cleanLine.Length - spaceIndex));
                            activeSprite.Border = border;
                        }
                        break;
                        case string a when a.StartsWith("spriteid"):
                        {
                            var spriteId = cleanLine.Split(' ')[1];
                            activeSprite.SpriteId = spriteId;
                        }
                        break;
                        case string a when a.StartsWith("internalid"):
                        {
                            var internalId = cleanLine.Split(' ')[1];
                            activeSprite.InternalId = long.Parse(internalId);
                        }
                        break;
                    }
                }

                if (cleanLine.StartsWith("sprites:"))
                {
                    parsingSpriteDepth = depth;
                    parsingSprites = true;
                }
            }

            return sprites;
        }
    }
}
