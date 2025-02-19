using System;
using UnityEngine;

namespace TableForge.UI
{
    internal static class NameResolver
    {
        public static string ResolveHeaderName(CellAnchor header, TableHeaderVisibility visibility)
        {
            return visibility switch
            {
                TableHeaderVisibility.Hidden => string.Empty,
                TableHeaderVisibility.ShowEmpty => string.Empty,
                TableHeaderVisibility.ShowHeaderName => header.Name,
                TableHeaderVisibility.ShowHeaderNumber => header.Position.ToString(),
                TableHeaderVisibility.ShowHeaderLetter => header.LetterPosition,
                TableHeaderVisibility.ShowHeaderLetterAndName => $"{header.LetterPosition} | {header.Name}",
                TableHeaderVisibility.ShowHeaderNumberAndName => $"{header.Position} | {header.Name}",
                _ => string.Empty
            };
        }
        
        public static string ResolveHeaderStyledName(CellAnchor header, TableHeaderVisibility visibility)
        {
            return visibility switch
            {
                TableHeaderVisibility.Hidden => string.Empty,
                TableHeaderVisibility.ShowEmpty => string.Empty,
                TableHeaderVisibility.ShowHeaderName => $"<b>{header.Name}</b>",
                TableHeaderVisibility.ShowHeaderNumber => $"<b>{header.Position}</b>",
                TableHeaderVisibility.ShowHeaderLetter => $"<b>{header.LetterPosition}</b>",
                TableHeaderVisibility.ShowHeaderLetterAndName => $"{header.LetterPosition} | <b>{header.Name}</b>",
                TableHeaderVisibility.ShowHeaderNumberAndName => $"{header.Position} | <b>{header.Name}</b>",
                _ => string.Empty
            };
        }
        
        public static string ResolveLayerMaskName(LayerMask mask)
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
        
        public static string ResolveFlagsEnumName(Type enumType, int value)
        {
            if (value == -1)
            {
                return "Everything";
            }

            if (value == 0)
            {
                if(Enum.IsDefined(enumType, 0))
                {
                    return Enum.GetName(enumType, 0);
                }
                return "Nothing";
            }

            string res = string.Empty;
            foreach (var name in Enum.GetNames(enumType))
            {
                int enumValue = (int)Enum.Parse(enumType, name);
                if ((value & enumValue) != 0)
                {
                    if (!string.IsNullOrEmpty(res))
                    {
                        res += ", ";
                    }
                    res += name.ConvertToProperCase();
                }
            }

            return res;
        }
        
    }
}