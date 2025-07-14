using System;
using System.Linq;
using UnityEngine;

namespace TableForge.Editor.Serialization
{
    [Serializable]
    internal class SerializableGradient
    {
        public SerializableColorKey[] colorKeys;
        public SerializableAlphaKey[] alphaKeys;
        public GradientMode mode;

        public SerializableGradient(Gradient gradient)
        {
            if (gradient == null)
            {
                colorKeys = Array.Empty<SerializableColorKey>();
                alphaKeys = Array.Empty<SerializableAlphaKey>();
                mode = GradientMode.Blend;
                return;
            }
            
            colorKeys = gradient.colorKeys.Select(x => new SerializableColorKey(x)).ToArray();
            alphaKeys = gradient.alphaKeys.Select(x => new SerializableAlphaKey(x)).ToArray();
            mode = gradient.mode;
        }

        public Gradient ToGradient()
        {
            var gradient = new Gradient
            {
                mode = mode
            };
            
            GradientColorKey[] actualColorKeys = colorKeys.Select(x => x.ToGradientColorKey()).ToArray();
            GradientAlphaKey[] actualAlphaKeys = alphaKeys.Select(x => x.ToGradientAlphaKey()).ToArray();
            gradient.SetKeys(actualColorKeys, actualAlphaKeys);
            return gradient;
        }
    }
    
    [Serializable]
    internal struct SerializableAlphaKey
    {
        public float time;
        public float alpha;

        public SerializableAlphaKey(GradientAlphaKey key)
        {
            time = key.time;
            alpha = key.alpha;
        }

        public GradientAlphaKey ToGradientAlphaKey()
        {
            return new GradientAlphaKey(alpha, time);
        }
    }
    
    [Serializable]
    internal struct SerializableColorKey
    {
        public float time;
        public SerializableColor color;

        public SerializableColorKey(GradientColorKey key)
        {
            time = key.time;
            color = new SerializableColor(key.color);
        }

        public GradientColorKey ToGradientColorKey()
        {
            return new GradientColorKey(color.ToColor(), time);
        }
    }
}