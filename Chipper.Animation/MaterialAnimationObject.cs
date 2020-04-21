using UnityEngine;
using Unity.Mathematics;
using Chipper.Rendering;

namespace Chipper.Animation
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Chipper/Animation/Material Animation Object")]
    public class MaterialAnimationObject : ScriptableObject
    {
        // @IMPORTANT: Do not touch these
        public CurveType           CurveType;      
        public string              Property;
        public float4              InitialValue;
        public float4              FinalValue;
        public float               Duration;
        public MaterialUpdateType  Type;
        public AnimationTransition TransitionType;
    
        public MaterialAnimation Component
        {
            get
            {
                return new MaterialAnimation
                {                
                    Time = 0,
                    FinalValue = FinalValue,
                    InitialValue = InitialValue,
                    PropertyID = Shader.PropertyToID(Property),
                    Transition = TransitionType,
                    Type = Type,
                    Curve = CurveType,
                    Duration = Duration,
                };
            }
        }
    }
}
