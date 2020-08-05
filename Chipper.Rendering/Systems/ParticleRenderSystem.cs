using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Profiling;
using System.Collections.Generic;
using System;

namespace Chipper.Rendering
{

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class ParticleRenderSystem : ComponentSystem
    {
        static readonly ProfilerMarker k_ProfileParticleRenderUpdate = new ProfilerMarker("ParticleRender.Update");
        static readonly ProfilerMarker k_ProfileParticleRenderCreate = new ProfilerMarker("ParticleRender.Create");
        static readonly ProfilerMarker k_ProfileParticleRenderDestroy = new ProfilerMarker("ParticleRender.Destroy");

        struct ParticleInstance
        {
            public bool IsNull => GameObject == null;

            public bool           WasAlive; 
            public bool           IsLinkedToEntity;
            public Entity         LinkedEntity;
            public GameObject     GameObject;
            public Transform      Transform;
            public ParticleSystem ParticleSystem;
            public ParticleDestroyMethod DestroyMethod;

            public void Set(float3 position, float angle)
            {
                Transform.position    = RenderUtil.GetRenderPosition(position);
                Transform.eulerAngles = new Vector3(0, 0, angle);
            }
        }

        Transform m_RootTransform;
        ParticleInstance[] m_ParticleInstances;
        Stack<int> m_EmptyIndexes;

        EntityQuery m_UpdateGroup;
        EntityQuery m_DestroyGroup;
        EntityQuery m_CreateGroup;

        protected override void OnCreate()
        {
            m_RootTransform = new GameObject("ParticleRoot").GetComponent<Transform>();
            m_ParticleInstances = new ParticleInstance[50];

            m_EmptyIndexes = new Stack<int>(50);
            for (int i = 0; i < 50; i++)
                m_EmptyIndexes.Push(i);
       

            m_CreateGroup = GetEntityQuery(
                ComponentType.ReadOnly(typeof(LocalToWorld)),
                ComponentType.ReadOnly(typeof(RotationEulerXYZ)),
                ComponentType.ReadOnly(typeof(ParticleComponent)),
                ComponentType.Exclude(typeof(ParticleRenderIndex)));

            m_UpdateGroup = GetEntityQuery(
                ComponentType.ReadOnly(typeof(LocalToWorld)),
                ComponentType.ReadOnly(typeof(RotationEulerXYZ)),
                ComponentType.ReadOnly(typeof(ParticleComponent)),
                ComponentType.ReadOnly(typeof(ParticleRenderIndex)));

            m_DestroyGroup = GetEntityQuery(
                ComponentType.ReadOnly(typeof(ParticleRenderIndex)),
                ComponentType.Exclude(typeof(LocalToWorld)));
        }

        protected override void OnUpdate()
        {
            FixParticleInstances(EntityManager);

            k_ProfileParticleRenderCreate.Begin();
            InstantiateSystems();
            k_ProfileParticleRenderCreate.End();

            k_ProfileParticleRenderUpdate.Begin();
            UpdateSystems();
            k_ProfileParticleRenderUpdate.End();

            k_ProfileParticleRenderDestroy.Begin();
            DestroySystems();
            k_ProfileParticleRenderDestroy.End();
        }

    
        void UpdateSystems()
        {
            if (m_UpdateGroup.IsEmptyIgnoreFilter)
                return;

            var chunks = m_UpdateGroup.CreateArchetypeChunkArray(Allocator.TempJob);
            var indexType = GetComponentTypeHandle<ParticleRenderIndex>(true);
            var localToWorldType = GetComponentTypeHandle<LocalToWorld>(true);
            var rotationType = GetComponentTypeHandle<RotationEulerXYZ>(true);

            for (int i = 0; i < chunks.Length; i++)
            {
                var chunk = chunks[i];
                var indexes = chunk.GetNativeArray(indexType);
                var localToWorlds = chunk.GetNativeArray(localToWorldType);
                var rotations = chunk.GetNativeArray(rotationType);

                for (int j = 0; j < chunk.Count; j++)
                {
                    var index    = indexes[j].Value;
                    var position = localToWorlds[j].Position;
                    var rotation = rotations[j].Value;
                    var instance = m_ParticleInstances[index];
                    instance.Set(position, rotation.z);                
                }
            }
            chunks.Dispose();
        }

        void InstantiateSystems()
        {
            if (m_CreateGroup.IsEmptyIgnoreFilter)
                return;

            var entities = m_CreateGroup.ToEntityArray(Allocator.TempJob);
            EntityManager.AddComponent(m_CreateGroup, typeof(ParticleRenderIndex));
            for(int i = 0; i < entities.Length; i++)
            {
                var entity   = entities[i];
                var particle = EntityManager.GetComponentData<ParticleComponent>(entity);
                var index    = GetAvailableIndex();

                CreateInstanceAtIndex(entity, index, particle);
                EntityManager.SetComponentData(entity, new ParticleRenderIndex { Value = index, });
            }
            entities.Dispose();        
        }

        void DestroySystems()
        {
            if (m_DestroyGroup.IsEmptyIgnoreFilter)
                return;

            var indexes = m_DestroyGroup.ToComponentDataArray<ParticleRenderIndex>(Allocator.TempJob);

            for (int i = 0; i < indexes.Length; i++)
                DestroyInstanceAtIndex(indexes[i].Value);

            indexes.Dispose();
            EntityManager.RemoveComponent(m_DestroyGroup, typeof(ParticleRenderIndex));
        }

        void FixParticleInstances(EntityManager dstManager)
        {
            var invalidEntities = new NativeList<Entity>(4, Allocator.Temp);
            for(int i = 0; i < m_ParticleInstances.Length; i++)
            {
                ref var instance = ref m_ParticleInstances[i];

                // If ParticleSystem got destroyed by ParticleSystem and not RenderSystem
                // LinkedEntity needs to be destroyed
                if(instance.IsLinkedToEntity && instance.GameObject == null)
                {
                    instance.IsLinkedToEntity = false;
                    invalidEntities.Add(instance.LinkedEntity);
                }
           
                // If ParticleSystem got destroyed, release the index back to pool
                if(instance.GameObject == null && instance.WasAlive)
                    m_EmptyIndexes.Push(i);

                instance.WasAlive = instance.GameObject != null;
            }

            if(invalidEntities.Length > 0)
                dstManager.DestroyEntity(invalidEntities);

            invalidEntities.Dispose();
        }

        int GetAvailableIndex()
        {
            if(m_EmptyIndexes.Count == 0)
            {
                var initialSize = m_ParticleInstances.Length;
                var newSize = m_ParticleInstances.Length * 2;
                Array.Resize(ref m_ParticleInstances, newSize);
                for(int i = initialSize; i < newSize; i++)
                    m_EmptyIndexes.Push(i);
            }

            return m_EmptyIndexes.Pop();
        }

        ParticleInstance CreateInstanceAtIndex(Entity entity, int index, ParticleComponent particle)
        {
            var prefab = ParticleConverter.GetPrefab(particle.Id);
            var gameObject = GameObject.Instantiate(prefab, m_RootTransform);
            m_ParticleInstances[index] = new ParticleInstance
            {
                DestroyMethod = particle.DestroyMethod,
                WasAlive = true,
                LinkedEntity = entity,
                IsLinkedToEntity = true,
                GameObject = gameObject,
                Transform = gameObject.transform,
                ParticleSystem = gameObject.GetComponent<ParticleSystem>(),
            };
            return m_ParticleInstances[index];
        }

        void DestroyInstanceAtIndex(int index)
        {
            ref var instance = ref m_ParticleInstances[index];
            var destroyMethod = instance.DestroyMethod;
            if (instance.IsLinkedToEntity)
            {
                instance.IsLinkedToEntity = false;
                if(destroyMethod == ParticleDestroyMethod.ByEntity)
                    GameObject.Destroy(instance.GameObject);
                else
                    instance.ParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}
