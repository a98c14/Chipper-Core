using Chipper.Prefabs.Attributes;
using Chipper.Prefabs.Data;
using Chipper.Prefabs.Parser;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Chipper.Prefabs
{
    public class AssetManager
    {
        public static List<IUnityAssetCache> AssetCaches
        {
            get
            {
                return AssetCacheManager.Main.Caches;
            }
        }

        public static string[] GetModuleJsonData()
        {
            var type = typeof(IPrefabModule);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface)
                .ToList();

            var settings = new JsonSerializerSettings
            {
                MaxDepth = 10,
            };

            var jsons = new string[types.Count];
            for (var i = 0; i < types.Count; i++)
            {
                var dict = new Dictionary<string, object>
                {
                    { "Name", types[i].Name },
                    { "Structure", TypeParser.GetParts(types[i]) }
                };

                var formerName = types[i].GetCustomAttribute<FormerNameAttribute>();
                if(formerName != null)
                    dict["FormerName"] = formerName.Name;
                var json = JsonConvert.SerializeObject(dict, settings);
                jsons[i] = json;
            }

            return jsons;
        }

        public static TextureMetaData[] GetSpriteData(string texturePath)
        {
            var metaFiles = Directory.GetFiles(texturePath, "*.png.meta", SearchOption.AllDirectories);
            var textures = new TextureMetaData[metaFiles.Length];
            for (int i = 0; i < metaFiles.Length; i++)
            {
                var metaFile = metaFiles[i];
                var directory = Path.GetDirectoryName(metaFile);
                var png = Path.GetFileNameWithoutExtension(metaFile);
                var name = Path.GetFileNameWithoutExtension(png);
                var guid = ParseGuid(metaFile);
                var texture = File.ReadAllBytes(Path.Combine(directory, png));
                var sprites = TextureParser.ParseSpriteMetaData(metaFile);
                textures[i] = new TextureMetaData
                {
                    Guid = guid,
                    Name = name,
                    Sprites = sprites,
                    Texture = texture,
                };
            }
            return textures;
        }

        private static string ParseGuid(string path)
        {
            foreach (var line in File.ReadLines(path))
            {
                var cleanLine = line.TrimStart().ToLower();
                cleanLine.StartsWith("guid");
                return cleanLine.Split(' ')[1];
            }
            return "-";
        }
    }
}


