using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;
using Chipper.Transforms;

namespace Chipper.Rendering
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class ShadowRenderSystem : ComponentSystem
    {
        struct ShadowInstance
        {
            public bool       IsActive;
            public Transform  Transform;
            public GameObject GameObject;
        }

        Transform        m_RootTransform;
        GameObject       m_RenderObject;
        EntityQuery      m_RenderGroup;
        ShadowInstance[] m_Objects;

        protected override void OnCreate()
        {
            m_RenderObject = RenderSettings.Main.Shadow;

            if (m_RenderObject == null)
            {
                Debug.LogWarning("Shadow prefab is null! Shadows will not be rendered.");
                Enabled = false;
                return;
            }

            m_RenderGroup = GetEntityQuery(ComponentType.ReadOnly(typeof(Position2D)), ComponentType.ReadOnly(typeof(Shadow)));
            m_Objects     = new ShadowInstance[RenderSettings.Main.ShadowPoolSize];
            m_RootTransform = new GameObject("ShadowPool").transform;

            for (int i = 0; i < m_Objects.Length; i++)
            {
                m_Objects[i].IsActive   = false;
                m_Objects[i].GameObject = GameObject.Instantiate(m_RenderObject, m_RootTransform);
                m_Objects[i].Transform  = m_Objects[i].GameObject.transform;

                m_Objects[i].GameObject.SetActive(false);
            }
        }

        protected override void OnUpdate()
        {
            var count     = m_RenderGroup.CalculateEntityCount();
            var positions = m_RenderGroup.ToComponentDataArray<Position2D>(Allocator.TempJob);
            var shadows   = m_RenderGroup.ToComponentDataArray<Shadow>(Allocator.TempJob);
       
            // Resize object pool if needed
            if (count > m_Objects.Length)
            {
                var newPool = new ShadowInstance[m_Objects.Length * 2];
                m_Objects.CopyTo(newPool, 0);
                for(int i = m_Objects.Length; i < newPool.Length; i++)
                {
                    newPool[i].IsActive   = false;
                    newPool[i].GameObject = GameObject.Instantiate(m_RenderObject, m_RootTransform);
                    newPool[i].Transform  = newPool[i].GameObject.transform;

                    newPool[i].GameObject.SetActive(false);
                }
                m_Objects = newPool;
            }

            // Set object data
            for(int i = 0; i < m_Objects.Length; i++)
            {
                if(i < count)
                {
                    var position = positions[i].Value;
                    var shadow   = shadows[i];
                    var scale    = shadow.Scale * Constant.ShadowScaleMultiplier;

                    m_Objects[i].GameObject.SetActive(true);
                    m_Objects[i].IsActive             = true;
                    m_Objects[i].Transform.position   = new float3(position.x + shadow.Offset.x, position.y + shadow.Offset.y, 0);
                    m_Objects[i].Transform.localScale = new float3(scale.x, scale.y, 1);
                }
                else if(m_Objects[i].IsActive)
                {
                    m_Objects[i].IsActive = false;
                    m_Objects[i].GameObject.SetActive(false);
                }
            }

            positions.Dispose();
            shadows.Dispose();
        }


    }
}
