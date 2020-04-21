using UnityEngine;
using System.Collections.Generic;

public static class TrailConverter
{
    static bool         m_Initialized;
    static string       m_LoadPath = "TrailRenderers/";
    static GameObject[] m_TrailRenderers;
    static Dictionary<GameObject, TrailComponent> m_ComponentTable;

    public static void LoadAssets()
    {
        var particleSystems = Resources.LoadAll<ParticleSystem>(m_LoadPath);
        m_TrailRenderers = new GameObject[particleSystems.Length];
        m_ComponentTable = new Dictionary<GameObject, TrailComponent>();

        for (int i = 0; i < particleSystems.Length; i++)
        {
            var gameObject = particleSystems[i].gameObject;
            m_TrailRenderers[i] = gameObject;
            m_ComponentTable.Add(gameObject, new TrailComponent
            {
                Id = i,
            });
        }
    }

    public static bool GetComponent(GameObject gameObject, out TrailComponent component)
    {
        if (!m_Initialized)
            LoadAssets();

        if (m_ComponentTable.ContainsKey(gameObject))
        {
            component = m_ComponentTable[gameObject];
            return true;
        }
        component = new TrailComponent();
        return false;
    }

    public static GameObject GetPrefab(int id)
    {
        if (!m_Initialized)
            LoadAssets();

        return m_TrailRenderers[id];
    }
}
