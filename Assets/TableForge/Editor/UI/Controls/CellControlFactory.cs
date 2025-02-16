using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal static class CellControlFactory
    {
        private static Dictionary<Type, ConstructorInfo> _cellControlConstructors = new Dictionary<Type, ConstructorInfo>();

        public static VisualElement Create(Cell cell, TableControl tableControl)
        {
            var cellControlType = CellStaticData.GetCellControlType(cell.GetType());
            
            var constructor = GetCellControlConstructor(cell.GetType(), new object[] {cell, tableControl});

            if (constructor != null)
                return (CellControl)constructor.Invoke(new object[] { cell, tableControl});

            Debug.LogError($"{cellControlType.Name} lacks required constructor.");
            return null;
        }
        
        private static ConstructorInfo GetCellControlConstructor(Type cellType, object[] args)
        {
            if (_cellControlConstructors.ContainsKey(cellType))
                return _cellControlConstructors[cellType];
            
            var cellControlType = CellStaticData.GetCellControlType(cellType);
            var constructor = cellControlType.GetConstructor(args.Select(x => x.GetType()).ToArray());
            _cellControlConstructors.Add(cellType, constructor);
            return constructor;
        }    
    }
}