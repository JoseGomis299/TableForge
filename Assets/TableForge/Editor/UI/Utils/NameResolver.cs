using System;
using UnityEngine;

namespace TableForge.Editor.UI
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
                TableHeaderVisibility.ShowHeaderNumberBase0 => (header.Position - 1).ToString(),
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
                TableHeaderVisibility.ShowHeaderNumberBase0 => $"<b>{header.Position - 1}</b>",
                TableHeaderVisibility.ShowHeaderLetterAndName => $"{header.LetterPosition} | <b>{header.Name}</b>",
                TableHeaderVisibility.ShowHeaderNumberAndName => $"{header.Position} | <b>{header.Name}</b>",
                _ => string.Empty
            };
        }
        
        public static string ResolveLayerMaskName(LayerMask mask)
        {
            return mask.ResolveName();
        }
        
        public static string ResolveFlagsEnumName(Type enumType, int value)
        {
            return enumType.ResolveFlaggedEnumName(value);
        }
        
    }
}