using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System;
using System.Linq;
using Newtonsoft.Json;
using Chipper.Prefabs.Parser;
using Chipper.Prefabs.Network;
using Chipper.Prefabs.Data;
using Chipper.Prefabs.Types;
using Chipper.Animation;

namespace Chipper.Prefabs.Conversion
{
    [AlwaysUpdateSystem]
    public class PrefabConversionSystem : ComponentSystem, IPrefabConversionSystem
    {
        public List<PrefabEntity> PrefabEntities { get; private set; }
        Dictionary<string, int> m_IdNameMap;
        Dictionary<string, PrefabEntity> m_EntityNameMap;
        Dictionary<int, PrefabEntity> m_EntityIdMap;

        // Maps the prefab part names to prefab part ids used by Hyperion
        Dictionary<string, int> m_ModuleNameIdMap = new Dictionary<string, int>();

        // Maps prefab part ids to actual IPrefabPart implementations;
        Dictionary<int, IPrefabModule> m_ComponentMap = new Dictionary<int, IPrefabModule>();

        AssetCache m_AssetCache;

        protected async override void OnCreate()
        {
            m_EntityNameMap = new Dictionary<string, PrefabEntity>();
            m_EntityIdMap = new Dictionary<int, PrefabEntity>();
            m_IdNameMap = new Dictionary<string, int>();
            m_AssetCache = AssetCache.Main;
            PrefabEntities = new List<PrefabEntity>();
            var client = new HyperionClient();

            LoadModules();

            // Map module names to module ids
            var dbModules = await client.GetModules();
            foreach(var module in dbModules)
                m_ModuleNameIdMap[module.Name] = module.Id;

            // Load prefabs and create prefab entities
            //var prefabs = await client.GetPrefabsDetailed();
            //foreach (var prefab in prefabs)
            //{
            //    var entity = EntityManager.CreateEntity(typeof(Prefab));
            //    EntityManager.SetName(entity, prefab.Name);

            //    // TODO(selim): Instead of serializing to json and deserializing just convert
            //    // from dictionary to desired type directly
            //    foreach (var (name, value) in PrefabParser.SerializePrefabModules(prefab))
            //    {
            //        var internalId = m_IdNameMap[name];
            //        var m = m_ComponentMap[internalId];
            //        var js = (IPrefabModule)JsonConvert.DeserializeObject(value, m.GetType(), new JsonSerializerSettings
            //        {
            //            NullValueHandling = NullValueHandling.Ignore,
            //            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            //        });
            //        js.Convert(entity, EntityManager, this);
            //    }

            //    SavePrefabEntity(prefab, entity);
            //}

            // Fetch sprite ids from backend
            var sprites = await client.GetSpritesAsync();
            foreach(var sprite in sprites)
            {
                //var assets = m_AssetCache.(sprite.InternalId);
            }
            

            // Map backend sprites to unity sprites (asset cache)

            // Fetch animations from backend
        }


        public int GetID(string name)
        {
            if (m_IdNameMap.ContainsKey(name.ToLower()))
            {
                return m_IdNameMap[name];
            }

            Debug.LogWarning($"There is no ID for given name: {name}");
            return -1;
        }

        public Entity GetEntity(int id)
        {
            Debug.Assert(m_EntityIdMap.ContainsKey(id), $"There is no entity prefab for given ID: {id}");
            return m_EntityIdMap[id].Entity;
        }

        public void LoadModules()
        {
            var type = typeof(IPrefabModule);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface)
                .ToList();

            for (var i = 0; i < types.Count; i++)
            {
                var instance = Activator.CreateInstance(types[i]);
                m_ComponentMap.Add(i, (IPrefabModule)instance);
                m_IdNameMap[types[i].Name] = i;
            }
        }

        protected override void OnUpdate()
        {
            
        }

        public bool TryGetPrefabEntity(int prefabId, out PrefabEntity prefabEntity)
        {
            return m_EntityIdMap.TryGetValue(prefabId, out prefabEntity);
        }

        public bool TryGetPrefabEntity(string name, out PrefabEntity prefabEntity)
        {
            return m_EntityNameMap.TryGetValue(name, out prefabEntity);
        }

        public bool TryGetEntity(int prefabId, out Entity entity)
        {
            if(m_EntityIdMap.TryGetValue(prefabId, out var prefabEntity))
            {
                entity = prefabEntity.Entity;
                return true;
            };

            Debug.LogWarning($"Entity with Id: {prefabId} could not be found.");
            entity = default;
            return false;
        }

        public bool TryGetEntity(string name, out Entity entity)
        {
            if (m_EntityNameMap.TryGetValue(name, out var prefabEntity))
            {
                entity = prefabEntity.Entity;
                return true;
            };

            Debug.LogWarning($"Entity with name: {name} could not be found.");
            entity = default;
            return false;
        }


        public UnityEngine.Sprite GetSprite(int spriteId)
        {
            return null;
        }

        public int GetSpriteId(UnityEngine.Sprite sprite)
        {
            return 0;
        }

        public int GetMaterialId(Material material)
        {
            return 0;
        }

        public int GetUnityRenderLayerId(RenderLayer layer)
        {
            return 0;
        }

        public int GetUnitySortingLayerId(RenderSortLayer layer)
        {
            return 0;
        }

        public Animation2D GetAnimation(int id)
        {
            return new Animation2D();
        }

        public MaterialAnimation GetMaterialAnimation(int id)
        {
            return new MaterialAnimation();
        }

        /* TODO(selim): Do not forget!!!
         * Below code should run when prefab has a parent.
         * ```
         * if (hasParent)
         * {
         *     var parent = GetPrimaryEntity(transform.parent);
         *     DstEntityManager.AddComponentData(entity, new Parent2D { Value = parent });
         *     DstEntityManager.RemoveComponent<LocalToParent>(entity);
         *     DstEntityManager.RemoveComponent<Parent>(entity);
         * }
         * ```
         */


        /// <summary>
        /// Stores the given prefab and entity to internal data structures
        /// </summary>
        /// <param name="prefab">Prefab to save</param>
        /// <param name="entity">Converted prefab entity</param>
        private void SavePrefabEntity(Data.Response.Prefab prefab, Entity entity)
        {
            var p = new PrefabEntity
            {
                Id = prefab.Id,
                Name = prefab.Name,
                Entity = entity,
            };
            m_IdNameMap.Add(p.Name, p.Id);
            m_EntityNameMap.Add(p.Name, p);
            m_EntityIdMap.Add(p.Id, p);
            PrefabEntities.Add(p);
        }
    }
}
