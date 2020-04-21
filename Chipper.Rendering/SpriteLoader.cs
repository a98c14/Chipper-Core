using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Chipper.Rendering
{
    [CreateAssetMenu(menuName = "Sprite Loader")]
    public class SpriteLoader : ScriptableObject, ISerializationCallbackReceiver
    {
        public static SpriteLoader Main
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = Resources.LoadAll<SpriteLoader>("").FirstOrDefault();
                    Debug.Assert(m_Instance != null, "Sprite Loader could not be found!");
                }

                return m_Instance;
            }
        }
        static SpriteLoader m_Instance = null;

        public string SpritePath = "Art/";
        [SerializeField, HideInInspector] Sprite[] m_Sprites;
        Dictionary<Sprite, SpriteID> m_SpriteTable;

        public SpriteID GetSpriteID(Sprite sprite)
        {
            Debug.Assert(m_SpriteTable.ContainsKey(sprite),
                $"Given Sprite ({sprite.name} : {sprite.texture.name}) isn't loaded by Sprite Loader! " +
                $"Please put the given sprite inside \"{ SpritePath }\"");

            return m_SpriteTable[sprite];
        }

        public Sprite GetSprite(SpriteID id)
        {
            //Debug.Assert(id.Value >= 0 && id.Value < m_Sprites.Length,
            //    $"Sprite ID ({ id.Value }) is out of bounds!");

            return m_Sprites[id.Value];
        }

        public void OnAfterDeserialize()
        {
            m_SpriteTable = new Dictionary<Sprite, SpriteID>(m_Sprites.Length);
            for (int i = 0; i < m_Sprites.Length; i++)
                m_SpriteTable.Add(m_Sprites[i], new SpriteID(i));
        }

        public void OnBeforeSerialize() { }

#if UNITY_EDITOR
        public void RebuildCache()
        {
            m_Sprites = Resources.LoadAll<Sprite>(SpritePath);
            m_SpriteTable = new Dictionary<Sprite, SpriteID>(m_Sprites.Length);
            for (int i = 0; i < m_Sprites.Length; i++)
                m_SpriteTable.Add(m_Sprites[i], new SpriteID(i));
        }
#endif
    }
}
