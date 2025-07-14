using UnityEngine;

namespace TableForge.Editor.Serialization
{
    [System.Serializable]
    internal class SerializableColor
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public SerializableColor(Color color)
        {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }

        public Color ToColor()
        {
            return new Color(r, g, b, a);
        }
    }
}