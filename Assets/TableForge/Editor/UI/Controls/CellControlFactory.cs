using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TableForge.UI
{
    internal static class CellControlFactory
    {
        private static Dictionary<int, CellControl> _idToCellControl = new Dictionary<int, CellControl>();
        private static Dictionary<Type, ConstructorInfo> _cellControlConstructors = new Dictionary<Type, ConstructorInfo>();
        private static CellControlPool _cellControlPools = new CellControlPool();
        
        public static CellControl GetCellControlFromId(int id)
        {
            return _idToCellControl.TryGetValue(id, out var cellControl) ? cellControl : null;
        }

        public static CellControl Create(Cell cell, TableControl tableControl)
        {
            CellControl cellControl = _cellControlPools.GetCellControl(cell, tableControl);
            cellControl.Refresh(cell, tableControl);

            if(!_idToCellControl.TryAdd(cell.Id, cellControl))
                _idToCellControl[cell.Id] = cellControl;
            
            return cellControl;
        }
        
        public static void Release(CellControl cellControl)
        {
            _idToCellControl.Remove(cellControl.Cell.Id);
            _cellControlPools.Release(cellControl);
        }
        
        public static CellControl CreateCellControl(Cell cell, TableControl tableControl)
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