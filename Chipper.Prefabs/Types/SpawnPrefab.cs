using UnityEngine;

namespace Chipper.Prefabs.Types
{
    public class SpawnPrefab
    {
        public int Prefab;
        public Vector4 Offset;
    }

    public class SkillSpawnPrefab
    {
        public int Prefab;
        public Vector4 Offset;

        // Determines if skill properties (e.g Damage) can be affected by
        // item modifiers or not
        public bool IsProtected;
    }
}
