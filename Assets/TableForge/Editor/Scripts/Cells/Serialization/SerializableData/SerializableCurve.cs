using System.Linq;
using UnityEngine;

namespace TableForge
{
    [System.Serializable]
    internal class SerializableCurve
    {
        public SerializableKeyframe[] keys;
        public WrapMode preWrapMode;
        public WrapMode postWrapMode;

        public SerializableCurve(AnimationCurve curve)
        {
            keys = curve.keys.Select(x => new SerializableKeyframe(x)).ToArray();
            preWrapMode = curve.preWrapMode;
            postWrapMode = curve.postWrapMode;
        }

        public AnimationCurve ToAnimationCurve()
        {
            Keyframe[] actualKeys = keys.Select(x => x.ToKeyframe()).ToArray();
            var curve = new AnimationCurve(actualKeys)
            {
                preWrapMode = preWrapMode,
                postWrapMode = postWrapMode
            };
            return curve;
        }
    }
    
    [System.Serializable]
    internal struct SerializableKeyframe
    {
        public float time;
        public float value;
        public float inTangent;
        public float outTangent;
        public int weightedMode;
        public float inWeight;
        public float outWeight;

        public SerializableKeyframe(Keyframe key)
        {
            time = key.time;
            value = key.value;
            inTangent = key.inTangent;
            outTangent = key.outTangent;
            weightedMode = (int)key.weightedMode;
            inWeight = key.inWeight;
            outWeight = key.outWeight;
        }

        public Keyframe ToKeyframe()
        {
           return new Keyframe(time, value, inTangent, outTangent, inWeight, outWeight)
                {
                    weightedMode = (WeightedMode)weightedMode
                };
        }
    }
}