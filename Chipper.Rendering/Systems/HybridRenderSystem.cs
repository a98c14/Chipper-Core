using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Profiling;
using System.Collections.Generic;
using Chipper.Transforms;

namespace Chipper.Rendering
{
    [AlwaysUpdateSystem]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class HybridRenderSystem : ComponentSystem
    {
        static readonly ProfilerMarker k_ProfileRenderUpdate = new ProfilerMarker("Render.Update");
        static readonly ProfilerMarker k_ProfileRenderCreate = new ProfilerMarker("Render.Create");
        static readonly ProfilerMarker k_ProfileRenderDestroy = new ProfilerMarker("Render.Destroy");

        class RenderPool
        {
            public int Length => Instances.Length;
            public int AvailableIndexCount => EmptyIndexes.Count;
            public bool HasAvailableIndex  => AvailableIndexCount == 0;

            public RenderInstance[] Instances;
            public Stack<int> EmptyIndexes;
        }

        struct RenderInstance
        {
            public bool                  IsPixelPerfect;
            public bool                  IsMaterialDirty;
            public GameObject            GameObject;
            public Transform             Transform;
            public SpriteRenderer        Renderer;
            public MaterialPropertyBlock PropertyBlock;

            public void SetActive(bool b) => GameObject.SetActive(b);

            public void GetPropertyBlock(MaterialPropertyBlock propertyBlock)
            {
                Renderer.GetPropertyBlock(propertyBlock);
            }

            public void SetPropertyBlock(MaterialPropertyBlock propertyBlock)
            {
                IsMaterialDirty = true;
                Renderer.SetPropertyBlock(propertyBlock);
            }

            public void ClearMaterial()
            {
                PropertyBlock.Clear();
                SetPropertyBlock(PropertyBlock);
            }
        }

        struct ChunkComponentTypes
        {
            public ArchetypeChunkComponentType<SpriteID>           SpriteID;
            public ArchetypeChunkComponentType<RenderInfo>         RenderInfo;
            public ArchetypeChunkComponentType<RenderIndex>        RenderIndex;
            public ArchetypeChunkComponentType<Position2D>         Position;
            public ArchetypeChunkComponentType<Rotation2D>         Rotation;
            public ArchetypeChunkComponentType<Scale2D>            Scale;
            public ArchetypeChunkComponentType<RenderFlipState>    FlipState;
            public ArchetypeChunkBufferType<MaterialUpdateElement> MaterialProperties;
        }

        Transform                   m_Root;
        SpriteLoader                m_SpriteLoader;
        Dictionary<int, RenderPool> m_RenderPools;
        Dictionary<int, GameObject> m_PoolPrefabs;

        EntityQuery m_InstantiatedGroup;
        EntityQuery m_DestroyedGroup;
        EntityQuery m_UpdateGroup;
    
        protected override void OnCreate()
        {
            var poolInfo   = RenderSettings.Main.PoolInfo;
            m_Root         = new GameObject("RenderObjects").GetComponent<Transform>();
            m_RenderPools  = new Dictionary<int, RenderPool>();
            m_PoolPrefabs  = new Dictionary<int, GameObject>();
            m_SpriteLoader = SpriteLoader.Main;

            for(int i = 0; i < poolInfo.Length; i++)
            {
                var initialCount = poolInfo[i].PoolSize;
                var materialID   = poolInfo[i].Material.GetHashCode();
                var prefab       = poolInfo[i].Prefab;
            
                m_PoolPrefabs[materialID] = prefab;
                m_RenderPools[materialID] = CreatePool(prefab, m_Root, initialCount);
            }

            m_InstantiatedGroup = GetEntityQuery(
                ComponentType.ReadOnly(typeof(Position2D)),
                ComponentType.ReadOnly(typeof(RenderInfo)),
                ComponentType.ReadOnly(typeof(MaterialInfo)),
                ComponentType.Exclude(typeof(RenderIndex)),
                ComponentType.Exclude(typeof(NoRender)));

            m_UpdateGroup = GetEntityQuery(
                ComponentType.ReadOnly(typeof(Position2D)),
                ComponentType.ReadOnly(typeof(RenderInfo)),
                ComponentType.ReadOnly(typeof(RenderIndex)),
                ComponentType.ReadOnly(typeof(Scale2D)),
                ComponentType.ReadOnly(typeof(Rotation2D)),
                ComponentType.ReadOnly(typeof(MaterialInfo)),
                ComponentType.Exclude(typeof(NoRender)));

            m_DestroyedGroup = GetEntityQuery(
                ComponentType.ReadOnly(typeof(RenderIndex)),
                ComponentType.Exclude(typeof(Position2D)));
        }

        protected override void OnUpdate()
        {
            k_ProfileRenderCreate.Begin();            
            SetIndices();
            k_ProfileRenderCreate.End();

            k_ProfileRenderUpdate.Begin();
            UpdateRenderers();
            k_ProfileRenderUpdate.End();

            k_ProfileRenderDestroy.Begin();
            FreeRenderers();
            k_ProfileRenderDestroy.End();
        }

        void SetIndices()
        {
            if (m_InstantiatedGroup.IsEmptyIgnoreFilter)
                return;

            foreach (var kv in m_RenderPools)
            {
                var id = kv.Key;
                var pool = kv.Value;
                m_InstantiatedGroup.SetSharedComponentFilter(new MaterialInfo { MaterialID = id });

                if (m_InstantiatedGroup.IsEmptyIgnoreFilter)
                    continue;

                var entities = m_InstantiatedGroup.ToEntityArray(Allocator.TempJob);
                EntityManager.AddComponent(m_InstantiatedGroup, typeof(RenderIndex));

                for(int i = 0; i < entities.Length; i++)
                {
                    var entity = entities[i];
                    var index = GetAvailableIndex(id, pool);
                    EntityManager.SetComponentData(entity, new RenderIndex
                    {
                        Pool = id,
                        Value = index,
                    });

                    pool.Instances[index].SetActive(true);
                }

                entities.Dispose();
            }
        }

        void UpdateRenderers()
        {
            if (m_UpdateGroup.IsEmptyIgnoreFilter)
                return;

            var types = new ChunkComponentTypes
            {
                RenderIndex        = GetArchetypeChunkComponentType<RenderIndex>(true),
                Position           = GetArchetypeChunkComponentType<Position2D>(true),
                Rotation           = GetArchetypeChunkComponentType<Rotation2D>(true),
                SpriteID           = GetArchetypeChunkComponentType<SpriteID>(true),
                Scale              = GetArchetypeChunkComponentType<Scale2D>(true),
                RenderInfo         = GetArchetypeChunkComponentType<RenderInfo>(true),
                FlipState          = GetArchetypeChunkComponentType<RenderFlipState>(true),
                MaterialProperties = GetArchetypeChunkBufferType<MaterialUpdateElement>(false),
            };

            foreach (var kv in m_RenderPools)
            {
                var id = kv.Key;
                var pool = kv.Value;
                m_UpdateGroup.SetSharedComponentFilter(new MaterialInfo { MaterialID = id });
                var chunks = m_UpdateGroup.CreateArchetypeChunkArray(Allocator.TempJob);
                SetChunks(pool, chunks, types);
                chunks.Dispose();
            }
        }

        void FreeRenderers()
        {
            if (m_DestroyedGroup.IsEmptyIgnoreFilter)
                return;

            var indices = m_DestroyedGroup.ToComponentDataArray<RenderIndex>(Allocator.TempJob); 
            for(int i = 0; i < indices.Length; i++)
            {
                var index = indices[i];
                var pool = m_RenderPools[index.Pool];

                var instance = pool.Instances[index.Value];
                if (instance.IsMaterialDirty)
                    instance.ClearMaterial();
                instance.SetActive(false);

                pool.EmptyIndexes.Push(index.Value);
            }
            indices.Dispose();

            EntityManager.RemoveComponent<RenderIndex>(m_DestroyedGroup);
        }

        RenderPool CreatePool(GameObject prefab, Transform root, int size)
        {
            var pool = new RenderPool
            {
                EmptyIndexes = new Stack<int>(size),
                Instances = new RenderInstance[size],                
            };

            for (int i = 0; i < pool.Instances.Length; i++)
            {
                var gameObject                  = GameObject.Instantiate(prefab, root);
                pool.Instances[i].GameObject    = gameObject;
                pool.Instances[i].Renderer      = gameObject.GetComponent<SpriteRenderer>();
                pool.Instances[i].Transform     = gameObject.GetComponent<Transform>();
                pool.Instances[i].PropertyBlock = new MaterialPropertyBlock();
                gameObject.SetActive(false);
            }

            return pool;
        }

        int GetAvailableIndex(int id, RenderPool pool)
        {
            if (pool.AvailableIndexCount == 0)
                ResizePool(pool, (pool.Length + 1) * 2, m_PoolPrefabs[id], m_Root);

            return pool.EmptyIndexes.Pop();
        }

        void ResizePool(RenderPool pool, int newSize, GameObject prefab, Transform root)
        {
            var initialSize = pool.Length;
            System.Array.Resize(ref pool.Instances, newSize);

            for(int i = initialSize; i < newSize; i++)
            {
                var gameObject                  = GameObject.Instantiate(prefab, root);
                pool.Instances[i].GameObject    = gameObject;
                pool.Instances[i].Renderer      = gameObject.GetComponent<SpriteRenderer>();
                pool.Instances[i].Transform     = gameObject.GetComponent<Transform>();
                pool.Instances[i].PropertyBlock = new MaterialPropertyBlock();
                gameObject.SetActive(false);
                pool.EmptyIndexes.Push(i);
            }
        }

        void SetChunks(RenderPool pool, NativeArray<ArchetypeChunk> chunks, ChunkComponentTypes types)
        {
            for(int i = 0; i < chunks.Length; i++)
            {
                var chunk         = chunks[i];
                var sprites       = chunk.GetNativeArray(types.SpriteID);
                var positions     = chunk.GetNativeArray(types.Position);
                var indexes       = chunk.GetNativeArray(types.RenderIndex);
                var scales        = chunk.GetNativeArray(types.Scale);
                var rotations     = chunk.GetNativeArray(types.Rotation);
                var renderInfos   = chunk.GetNativeArray(types.RenderInfo);
                var flipStates    = chunk.GetNativeArray(types.FlipState);
            
                SetTransforms(pool.Instances, indexes, positions);
                SetScales(pool.Instances, indexes, scales);
                SetRotations(pool.Instances, indexes, rotations);
                SetRenderInfos(pool.Instances, indexes, renderInfos);
                SetFlipStates(pool.Instances, indexes, flipStates);
                SetSprites(pool.Instances, indexes, sprites);

                if (chunk.Has(types.MaterialProperties))
                {
                    var materialAccessors = chunk.GetBufferAccessor(types.MaterialProperties);
                    SetMaterialPropertyBlocks(pool.Instances, indexes, materialAccessors);
                }
            }
        }

        void SetTransforms(RenderInstance[] instances, NativeArray<RenderIndex> indexes, NativeArray<Position2D> positions)
        {
            for(int i = 0; i < positions.Length; i++)
            {
                var instance = instances[indexes[i].Value];
                var position = positions[i].Value;
                instance.Renderer.sortingOrder = (int)position.y * -1;
                instance.Transform.position = RenderUtil.GetRenderPosition(position);
            }
        }

        void SetScales(RenderInstance[] instances, NativeArray<RenderIndex> indexes, NativeArray<Scale2D> scales)
        {
            for (int i = 0; i < scales.Length; i++)
            { 
                var instance = instances[indexes[i].Value];
                var scale = scales[i];
                instance.Transform.localScale = new Vector3(scale.Value.x, scale.Value.y, 1);
            }
        }

        void SetRotations(RenderInstance[] instances, NativeArray<RenderIndex> indexes, NativeArray<Rotation2D> rotations)
        {
            for (int i = 0; i < rotations.Length; i++)
            { 
                var instance = instances[indexes[i].Value];
                instance.Transform.eulerAngles = new Vector3(0, 0, rotations[i].Value);
            }
        }

        void SetSprites(RenderInstance[] instances, NativeArray<RenderIndex> indexes, NativeArray<SpriteID> sprites)
        {
            for(int i = 0; i < sprites.Length; i++)
            {
                var instance = instances[indexes[i].Value];
                instance.Renderer.sprite = m_SpriteLoader.GetSprite(sprites[i]);
            }
        }

        void SetRenderInfos(RenderInstance[] instances, NativeArray<RenderIndex> indexes, NativeArray<RenderInfo> renderInfos)
        {
            for(int i = 0; i < renderInfos.Length; i++)
            {
                var instance = instances[indexes[i].Value];
                var renderInfo = renderInfos[i];
                instance.GameObject.layer = renderInfo.Layer;
                instance.Renderer.sortingLayerID = renderInfo.SortingLayer;
                instance.Renderer.color = renderInfo.Color;
            }
        }

        void SetFlipStates(RenderInstance[] instances, NativeArray<RenderIndex> indexes, NativeArray<RenderFlipState> flipStates)
        {
            for(int i = 0; i < flipStates.Length; i++)
            {
                var instance = instances[indexes[i].Value];
                var flipState = flipStates[i];
                instance.Renderer.flipX = flipState.flipX;
                instance.Renderer.flipY = flipState.flipY;
            }
        }

        void SetMaterialPropertyBlocks(RenderInstance[] instances, NativeArray<RenderIndex> indexes, BufferAccessor<MaterialUpdateElement> materialPropertyAccessors)
        {
            for (int i = 0; i < materialPropertyAccessors.Length; i++)
            { 
                var properties = materialPropertyAccessors[i].AsNativeArray();
                var instance = instances[indexes[i].Value];

                if(properties.Length > 0)
                {
                    instance.GetPropertyBlock(instance.PropertyBlock);
                    for(int materialIndex = 0; materialIndex < properties.Length; materialIndex++)
                    {
                        var material = properties[materialIndex];
                        var value = material.Value;
                        switch (material.Type)
                        {
                            case MaterialUpdateType.Color:
                                instance.PropertyBlock.SetColor(material.PropertyID, new Color(value.x, value.y, value.z ,value.w));
                                break;
                            case MaterialUpdateType.Float:
                                instance.PropertyBlock.SetFloat(material.PropertyID, value.x);
                                break;
                            case MaterialUpdateType.Vector:
                                instance.PropertyBlock.SetVector(material.PropertyID, value);
                                break;
                        }
                    }
                    instance.SetPropertyBlock(instance.PropertyBlock);
                }
                materialPropertyAccessors[i].Clear();
            }
        }
    }
}
