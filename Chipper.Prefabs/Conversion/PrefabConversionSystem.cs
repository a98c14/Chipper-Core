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
using Chipper.Transforms;
using static UnityEngine.EventSystems.EventTrigger;
using UnityEngine.UIElements;
using Unity.Mathematics;
using Chipper.Rendering;

namespace Chipper.Prefabs.Conversion
{
    [AlwaysUpdateSystem]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class PrefabConversionSystem : ComponentSystem, IPrefabConversionSystem
    {
        public List<PrefabEntity> PrefabEntities { get; private set; }
        Dictionary<string, int> m_IdNameMap;
        Dictionary<string, PrefabEntity> m_EntityNameMap;
        Dictionary<int, PrefabEntity> m_EntityIdMap;

        // Maps the prefab part names to prefab part ids used by Hyperion
        Dictionary<string, int> m_ModuleNameIdMap = new Dictionary<string, int>();

        // Maps prefab part ids to actual IPrefabPart implementations;
        Dictionary<int, IPrefabModule> m_ComponentMap = new Dictionary<int, IPrefabModule>();

        Data.Response.Prefab[] m_Prefabs;
        Data.Response.PrefabTransform[] m_Transforms;
        Data.Response.PrefabRenderer[] m_Renderers;
        AssetCacheManager m_AssetCache;
        bool m_IsInitialized;
        bool m_ReadyToInitialize;

        protected override async void OnCreate()
        {
            m_EntityNameMap = new Dictionary<string, PrefabEntity>();
            m_EntityIdMap = new Dictionary<int, PrefabEntity>();
            m_IdNameMap = new Dictionary<string, int>();
            m_AssetCache = AssetCacheManager.Main;
            PrefabEntities = new List<PrefabEntity>();
            var client = new HyperionClient(m_AssetCache.HyperionUrl);

            LoadModules();

            // Map module names to module ids
            var dbModules = await client.GetModulesAsync();
            // var dbModules = client.GetModules();
            foreach (var module in dbModules)
                m_ModuleNameIdMap[module.Name] = module.Id;

            // Load prefabs and create prefab entities
            m_Prefabs = await client.GetPrefabsDetailedAsync();

            // Load prefabs and create prefab entities
            m_Transforms = await client.GetPrefabTransformsAsync();
            m_Renderers= await client.GetPrefabRenderersAsync();
            m_ReadyToInitialize = true;
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
            if(m_IsInitialized || !m_ReadyToInitialize)
                return;
            
            // var prefabs = client.GetPrefabsDetailed();
            foreach (var prefab in m_Prefabs)
            {
                var entity = EntityManager.CreateEntity(typeof(Prefab));
                EntityManager.SetName(entity, prefab.Name);

                // TODO(selim): Instead of serializing to json and deserializing just convert
                // from dictionary to desired type directly
                foreach (var (name, value) in PrefabParser.SerializePrefabModules(prefab))
                {
                    var internalId = m_IdNameMap[name];
                    var m = m_ComponentMap[internalId];
                    Debug.Log($"Deserializing module {name}");
                    var module = (IPrefabModule)JsonConvert.DeserializeObject(value, m.GetType(), new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    });
                    module.Convert(entity, EntityManager, this);
                }

                SavePrefabEntity(prefab, entity);
            }

            // Set entity transforms
            foreach(var transform in m_Transforms)
            {
                var entity = m_EntityIdMap[transform.PrefabId].Entity;
                EntityManager.AddComponentData(entity, new Position2D { Value = transform.Position.Float3 });
                EntityManager.AddComponentData(entity, new Rotation2D { Value = transform.Rotation.Z });
                EntityManager.AddComponentData(entity, new Scale2D { Value = new float2(transform.Scale.X, transform.Scale.Y) });
            }

            // Set entity renderers
            foreach (var renderer in m_Renderers)
            {
                var entity = m_EntityIdMap[renderer.PrefabId].Entity;

                // If no material is attached to entity, do not render it
                if (renderer.MaterialAssetId == 0 || renderer.MaterialAssetId == null)
                    continue;

                // Set material
                var material = GetMaterial(renderer.MaterialAssetId ?? 0);
                EntityManager.AddSharedComponentData(entity, new MaterialInfo
                {
                    MaterialID = material.GetHashCode(),
                });

                // Set sprite
                var spriteIndex = 0;
                if (renderer.SpriteAssetId != null && renderer.SpriteAssetId > 0)
                {
                    spriteIndex = GetSpriteIndex(renderer.SpriteAssetId ?? 0);
                }
                EntityManager.AddComponentData(entity, new SpriteID
                {
                    Value = spriteIndex,
                });

                // Set render info
                EntityManager.AddComponentData(entity, new RenderInfo
                {
                    Color = new Color32(255, 255, 255, 255),
                    IsDefaultDirectionRight = !renderer.FlipX,
                    Layer = GetUnityRenderLayerId((RenderLayer)renderer.RenderLayer),
                    SortingLayer = GetUnitySortingLayerId((RenderSortLayer)renderer.RenderSortLayer),
                });

                EntityManager.AddComponentData(entity, new RenderFlipState
                {
                    flipX = renderer.FlipX,
                    flipY = renderer.FlipY,
                });
            }
            m_IsInitialized = true;
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
            if(id == 0) return new Animation2D();
            var anim = m_AssetCache.GetAnimation(id);
            return m_AssetCache.CreateAnimationBlob(anim);
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

        public Material GetMaterial(int materialAssetId) => m_AssetCache.GetAsset<Material>(materialAssetId);
        public UnityEngine.Sprite GetSprite(int spriteAssetId) => m_AssetCache.GetAsset<UnityEngine.Sprite>(spriteAssetId);
        public int GetSpriteIndex(int spriteId) => m_AssetCache.SpriteCache.GetAssetIndex(spriteId);
    }
}
