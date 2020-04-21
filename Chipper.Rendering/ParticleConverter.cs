using UnityEngine;
using System.Collections.Generic;

public static class ParticleConverter
{
    static bool                                      m_Initialized;
    static string                                    m_LoadPath = "ParticleSystems/";
    static GameObject[]                              m_ParticlePrefabs;
    static Dictionary<GameObject, ParticleComponent> m_ComponentTable;

    public static void LoadAssets()
    {
        var particleSystems = Resources.LoadAll<ParticleSystem>(m_LoadPath);
        m_ParticlePrefabs = new GameObject[particleSystems.Length];
        m_ComponentTable = new Dictionary<GameObject, ParticleComponent>();

        for(int i = 0; i < particleSystems.Length; i++)
        {
            var particleSystem = particleSystems[i];
            var gameObject = particleSystem.gameObject;
            m_ParticlePrefabs[i] = gameObject;

            m_ComponentTable.Add(gameObject, new ParticleComponent
            {
                DestroyMethod = GetParticleDestroyMethod(particleSystem),
                Id = i,
            });
        }
    }

    public static bool GetComponent(GameObject gameObject, out ParticleComponent component)
    {
        if(!m_Initialized)
            LoadAssets();

        if (m_ComponentTable.ContainsKey(gameObject))
        {
            component = m_ComponentTable[gameObject];
            return true;
        }
        component = new ParticleComponent();
        return false;
    }

    public static GameObject GetPrefab(int id)
    {
        if(!m_Initialized)
            LoadAssets(); 

        return m_ParticlePrefabs[id];
    }

    static ParticleDestroyMethod GetParticleDestroyMethod(ParticleSystem particleSystem)
    {
        switch (particleSystem.main.stopAction)
        {
            case ParticleSystemStopAction.Destroy:
                return ParticleDestroyMethod.ByGameObject;
            case ParticleSystemStopAction.None:
                return ParticleDestroyMethod.ByEntity;
            default:
                return ParticleDestroyMethod.ByGameObject;
        }
    }
}
