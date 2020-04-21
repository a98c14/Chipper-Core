using UnityEngine;
using System.Linq;

[System.Serializable]
public struct PooledObjectInfo
{ 
    public GameObject Prefab;
    public Material   Material;
    public int        PoolSize;
}

[CreateAssetMenu(menuName = "Settings/Render Settings")]
public class RenderSettings : ScriptableObject
{
    public static RenderSettings Main
    {
        get
        {
            if (k_Instance == null)
            {
                k_Instance = Resources.LoadAll<RenderSettings>("").FirstOrDefault();
                Debug.Assert(k_Instance != null, "Render settings could not be found!");
            }

            return k_Instance;
        }
    }
    static RenderSettings k_Instance = null;
    
    [Header("Custom Pools")]
    public PooledObjectInfo[] PoolInfo;

    [Header("Default Pools")]
    public GameObject Shadow;
    public int ShadowPoolSize;

}
