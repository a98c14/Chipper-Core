namespace Chipper.Prefabs.Data.Response
{
    public class Prefab
    {
        public int Id { get; set; }
        public string Name { get; set; }       
        public PrefabTransform Transform { get; set; }
        public PrefabRenderer Renderer { get; set; } 
        public Collider[] Colliders { get; set; }
        public PrefabModulePart[] Modules { get; set;}
    }
}