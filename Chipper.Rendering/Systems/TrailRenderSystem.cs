using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using System.Collections.Generic;
using Chipper.Transforms;
using Unity.Transforms;
using Unity.Profiling;
using Unity.Mathematics;
using System;

namespace Chipper.Rendering
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class TrailRenderSystem : ComponentSystem
    {
        static readonly ProfilerMarker k_ProfileTrailRenderUpdate = new ProfilerMarker("TrailRender.Update");
        static readonly ProfilerMarker k_ProfileTrailRenderCreate = new ProfilerMarker("TrailRender.Create");
        static readonly ProfilerMarker k_ProfileTrailRenderDestroy = new ProfilerMarker("TrailRender.Destroy");

        struct TrailInstance
        {
            public bool IsNull => GameObject == null;

            public GameObject    GameObject;
            public Transform     Transform;
            public TrailRenderer TrailRenderer;

            public void Set(float3 position, float angle)
            {
                Transform.position = RenderUtil.GetRenderPosition(position);
                Transform.eulerAngles = new Vector3(0, 0, angle);
            }
        }

        Transform       m_RootTransform;
        TrailInstance[] m_TrailInstances;
        Stack<int>      m_EmptyIndexes;

        EntityQuery m_UpdateGroup;
        EntityQuery m_DestroyGroup;
        EntityQuery m_CreateGroup;

        protected override void OnCreate()
        {
            m_RootTransform = new GameObject("TrailRoot").GetComponent<Transform>();
            m_TrailInstances = new TrailInstance[50];

            m_EmptyIndexes = new Stack<int>(50);
            for (int i = 0; i < 50; i++)
                m_EmptyIndexes.Push(i);

            m_CreateGroup = GetEntityQuery(
                ComponentType.ReadOnly(typeof(LocalToWorld)),
                ComponentType.ReadOnly(typeof(Rotation2D)),
                ComponentType.ReadOnly(typeof(TrailComponent)),
                ComponentType.Exclude(typeof(TrailRenderIndex)));

            m_UpdateGroup = GetEntityQuery(
                ComponentType.ReadOnly(typeof(LocalToWorld)),
                ComponentType.ReadOnly(typeof(Rotation2D)),
                ComponentType.ReadOnly(typeof(TrailComponent)),
                ComponentType.ReadOnly(typeof(TrailRenderIndex)));

            m_DestroyGroup = GetEntityQuery(
                ComponentType.ReadOnly(typeof(TrailRenderIndex)),
                ComponentType.Exclude(typeof(LocalToWorld)));
        }

        protected override void OnUpdate()
        {
            k_ProfileTrailRenderCreate.Begin();
            InstantiateSystems();
            k_ProfileTrailRenderCreate.End();

            k_ProfileTrailRenderUpdate.Begin();
            UpdateSystems();
            k_ProfileTrailRenderUpdate.End();

            k_ProfileTrailRenderDestroy.Begin();
            DestroySystems();
            k_ProfileTrailRenderDestroy.End();
        }

        void InstantiateSystems()
        {
            if (m_CreateGroup.IsEmptyIgnoreFilter)
                return;

            var entities = m_CreateGroup.ToEntityArray(Allocator.TempJob);
            EntityManager.AddComponent(m_CreateGroup, typeof(TrailRenderIndex)); // TODO: This might invalidate the entities array, need to check

            for (int i = 0; i < entities.Length; i++)
            {
                var entity   = entities[i];
                var position = EntityManager.GetComponentData<LocalToWorld>(entity).Position;
                var rotation = EntityManager.GetComponentData<Rotation2D>(entity).Value;
                var trail    = EntityManager.GetComponentData<TrailComponent>(entity);
                var index    = GetAvailableIndex();
                var instance = CreateInstanceAtIndex(index, trail.Id);
                instance.Set(position, rotation);

                EntityManager.SetComponentData(entity, new TrailRenderIndex
                {
                    Value = index,
                });
            }

            entities.Dispose();
        }

        void DestroySystems()
        {
            if(m_DestroyGroup.IsEmptyIgnoreFilter)
                return;

            var indexes = m_DestroyGroup.ToComponentDataArray<TrailRenderIndex>(Allocator.TempJob);
            for (int i = 0; i < indexes.Length; i++)
                StopInstanceAtIndex(indexes[i].Value);

            indexes.Dispose();
            EntityManager.RemoveComponent(m_DestroyGroup, typeof(TrailRenderIndex));
        }

        void UpdateSystems()
        {
            if(m_UpdateGroup.IsEmptyIgnoreFilter)
                return;

            var chunks = m_UpdateGroup.CreateArchetypeChunkArray(Allocator.TempJob);
            var entityType = GetEntityTypeHandle();
            var indexType = GetComponentTypeHandle<TrailRenderIndex>(true);
            var localToWorldType = GetComponentTypeHandle<LocalToWorld>(true);
            var rotationType = GetComponentTypeHandle<Rotation2D>(true);
            var invalidIndexes = new NativeList<Entity>(5, Allocator.Temp);

            for (int i = 0; i < chunks.Length; i++)
            {
                var chunk         = chunks[i];
                var entities      = chunk.GetNativeArray(entityType);
                var indexes       = chunk.GetNativeArray(indexType);
                var localToWorlds = chunk.GetNativeArray(localToWorldType);
                var rotations     = chunk.GetNativeArray(rotationType);

                for (int j = 0; j < chunk.Count; j++)
                {
                    var index = indexes[j].Value;
                    var entity = entities[j];
                    var position = localToWorlds[j].Position;
                    var rotation = rotations[j].Value;
                    var instance = m_TrailInstances[index];

                    // If instance object is destroyed outside render system
                    // destroy the linked entity and push it back to empty indexes
                    if (instance.IsNull)
                    {
                        invalidIndexes.Add(entity);
                        m_EmptyIndexes.Push(index);
                    }
                    else
                    {
                        instance.Set(position, rotation);
                    }
                }
            }

            chunks.Dispose();
        }


        int GetAvailableIndex()
        {
            if (m_EmptyIndexes.Count == 0)
            {
                var initialSize = m_TrailInstances.Length;
                var newSize = m_TrailInstances.Length * 2;
                Array.Resize(ref m_TrailInstances, newSize);
                for (int i = initialSize; i < newSize; i++)
                    m_EmptyIndexes.Push(i);
            }

            return m_EmptyIndexes.Pop();
        }

        void StopInstanceAtIndex(int index)
        {
            var instance = m_TrailInstances[index];
            instance.TrailRenderer.emitting = false;
            instance.TrailRenderer.autodestruct = true;
        }

        TrailInstance CreateInstanceAtIndex(int index, int id)
        {
            var prefab = TrailConverter.GetPrefab(id);
            var gameObject = GameObject.Instantiate(prefab, m_RootTransform);
            m_TrailInstances[index] = new TrailInstance
            {
                GameObject = gameObject,
                Transform = gameObject.transform,
                TrailRenderer = gameObject.GetComponent<TrailRenderer>(),
            };
            return m_TrailInstances[index];
        }
    }
}
