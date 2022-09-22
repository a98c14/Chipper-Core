using System;

namespace Chipper.Prefabs.Data.Response
{
    [Serializable]
    public class Animation
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public string Name { get; set; }
        public int Priority { get; set; }
        public int TransitionType { get; set; }

        /// <summary>
        /// Asset ids of sprites
        /// </summary>
        public int[] Sprites { get; set; }
    }
}