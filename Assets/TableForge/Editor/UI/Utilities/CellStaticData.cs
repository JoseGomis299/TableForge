using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TableForge.Editor.UI
{
    internal static class CellStaticData
    {
        private static readonly Dictionary<Type, Type> _cellToCellControl = new();
        private static readonly Dictionary<Type, TableAttributes> _subTableCellAttributes = new();
        private static readonly Dictionary<Type, CellAttributes> _cellAttributes = new();
        
        static CellStaticData()
        {
            DiscoverCellTypes();
        }
        
        private static void DiscoverCellTypes()
        {
            var assembly = Assembly.GetExecutingAssembly();

            foreach (var type in assembly.GetTypes())
            {
                if (typeof(CellControl).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    foreach (var attr in type.GetCustomAttributes<CellControlUsageAttribute>())
                    {
                        _cellToCellControl.Add(attr.CellAttributes.CellType, type);
                        _cellAttributes.Add(type, attr.CellAttributes);
                    }
                }
                
                if (typeof(SubTableCellControl).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    foreach (var attr in type.GetCustomAttributes<SubTableCellControlUsageAttribute>())
                    {
                        _subTableCellAttributes.Add(type, attr.TableAttributes);
                    }
                }
            }
        }
        
        public static Type GetCellType(Type cellControlType)
        {
            return _cellToCellControl.FirstOrDefault(x => x.Value == cellControlType).Key;
        }
        
        public static Type GetCellControlType(Type cellType)
        {
            if (_cellToCellControl.ContainsKey(cellType))
                return _cellToCellControl[cellType];
            
            throw new ArgumentException($"No cell control found for cell type {cellType.FullName}");
        }

        public static TableAttributes GetSubTableCellAttributes(Type subTableCellControlType)
        {
            if (_subTableCellAttributes.ContainsKey(subTableCellControlType))
                return _subTableCellAttributes[subTableCellControlType];

            throw new ArgumentException(
                $"No sub table cell attributes found for sub table cell control type {subTableCellControlType.FullName}");
        }

        public static CellAttributes GetCellAttributes(Type cellControlType)
        {
            if (_cellAttributes.ContainsKey(cellControlType))
                return _cellAttributes[cellControlType];

            throw new ArgumentException($"No cell attributes found for cell control type {cellControlType.FullName}");
        }
    }
}