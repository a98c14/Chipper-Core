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
        Dictionary<string, int> m_IdNameMap;
        Dictionary<string, Entity> m_EntityNameMap;
        Dictionary<int, Entity> m_EntityIdMap;

        // Maps the prefab part names to prefab part ids used by Hyperion
        Dictionary<string, int> m_PrefabPartNameIdMap = new Dictionary<string, int>();

        // Maps prefab part ids to actual IPrefabPart implementations;
        Dictionary<int, IPrefabModule> m_ComponentMap = new Dictionary<int, IPrefabModule>();

        int m_LoadId;

        protected override void OnCreate()
        {
            m_EntityNameMap = new Dictionary<string, Entity>();
            m_EntityIdMap = new Dictionary<int, Entity>();
            m_IdNameMap = new Dictionary<string, int>();

            GetAllComponentDatas();
            // ConvertPrefabs(prefabs.Length, prefabs);

            ParseTest();
        }

        public int AddEntityPrefab(string name, Entity entity)
        {
            m_IdNameMap.Add(name, m_LoadId);
            m_EntityNameMap.Add(name, entity);
            m_EntityIdMap.Add(m_LoadId, entity);
            m_LoadId++;
            return m_LoadId - 1;
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
            return m_EntityIdMap[id];
        }

        public void ParseTest()
        {
            // Life;
            // Shield;
            // Armor;
            // Barrier;
            // BarrierDecaySpeed;
            var testJson = "{ \"Life\": 1, \"Shield\": 3, \"Armor\": 5, \"Barrier\": 1, \"BarrierDecaySpeed\": 0 }";
            var entity = EntityManager.CreateEntity(typeof(Prefab));
            EntityManager.SetName(entity, "DynamicEntity");
            var m = m_ComponentMap[0];
            var js = (IPrefabModule)JsonConvert.DeserializeObject(testJson, m.GetType());
            IPrefabConversionSystem k = null; // TODO(selim): 
            js.Convert(entity, EntityManager, k);
            // m_ComponentMap[0].Convert(entity, EntityManager, testJson);
            // EntityManager.AddComponentData(entity, parsed);
        }

        public void GetAllComponentDatas()
        {
            var type = typeof(IPrefabModule);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface)
                .ToList();
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new PrefabModuleContractResolver(typeof(MonoBehaviour)),
            };

            var client = new HyperionClient();
            for (var i = 0; i < types.Count; i++)
            {
                var instance = Activator.CreateInstance(types[i]);
                var request = new ModuleRequest
                {
                    Name = types[i].Name,
                    Structure = instance,
                };
                var json = JsonConvert.SerializeObject(request, settings);
                client.CreatePrefabModule(JsonConvert.SerializeObject(request, settings));
                m_ComponentMap.Add(i, (IPrefabModule)instance);
            }
        }

        protected override void OnUpdate()
        {
            throw new NotImplementedException();
        }

        public Entity GetPrefabEntity(int prefabId)
        {
            throw new NotImplementedException();
        }

        public UnityEngine.Sprite GetSprite(int spriteId)
        {
            throw new NotImplementedException();
        }

        public int GetSpriteId(UnityEngine.Sprite sprite)
        {
            throw new NotImplementedException();
        }

        public int GetMaterialId(Material material)
        {
            throw new NotImplementedException();
        }

        public int GetUnityRenderLayerId(RenderLayer layer)
        {
            throw new NotImplementedException();
        }

        public int GetUnitySortingLayerId(RenderSortLayer layer)
        {
            throw new NotImplementedException();
        }

        public Animation2D GetAnimation(int id)
        {
            throw new NotImplementedException();
        }

        public MaterialAnimation GetMaterialAnimation(int id)
        {
            throw new NotImplementedException();
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
    }
}
