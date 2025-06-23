using UnityEngine;

namespace TableForge.Editor
{
    internal static class LayerMaskExtension
    {
        public static string ResolveName(this LayerMask mask)
        {
            if (mask.value == ~0) 
            {
                return "Everything";
            }

            if (mask.value == 0) 
            {
                return "Nothing";
            }

            var res = string.Empty;
            for (int i = 0; i < 32; i++)
            {
                int layerBit = 1 << i;
                if ((mask.value & layerBit) != 0)
                {
                    string layerName = LayerMask.LayerToName(i);
                    if (!string.IsNullOrEmpty(layerName))
                    {
                        if (!string.IsNullOrEmpty(res))
                        {
                            res += ", ";
                        }
                        res += layerName;
                    }
                }
            }

            return res;
        }
    }
    
}