using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TableForge.Editor.UI
{
    internal static class CellControlFactory
    {
        private static readonly Dictionary<int, CellControl> _idToCellControl = new();
        private static readonly Dictionary<Type, ConstructorInfo> _cellControlConstructors = new();
        private static readonly CellControlPool _cellControlPool = new();
        
        public static CellControl GetCellControlFromId(int id)
        {
            if (!_idToCellControl.TryGetValue(id, out var cellControl)) return null;
            if(cellControl.Cell.Id != id)
            {
                _idToCellControl.Remove(id);
                _idToCellControl.Add(cellControl.Cell.Id, cellControl);
                return null;
            }
                
            return cellControl;

        }

        public static CellControl GetPooled(Cell cell, TableControl tableControl)
        {
            CellControl cellControl = _cellControlPool.GetCellControl(cell, tableControl);

            if (!_idToCellControl.TryAdd(cell.Id, cellControl))
            {
                CellControl existingCellControl = _idToCellControl[cell.Id];
                if (tableControl.CellSelector.IsCellFocused(existingCellControl.Cell))
                {
                    existingCellControl.SetFocused(false);
                }
                
                _idToCellControl[cell.Id] = cellControl;
            }
            
            cellControl.Refresh(cell, tableControl);
            return cellControl;
        }
        
        public static void Release(CellControl cellControl)
        {
            _idToCellControl.Remove(cellControl.Cell.Id);
            _cellControlPool.Release(cellControl);
        }
        
        public static CellControl Create(Cell cell, TableControl tableControl)
        {
            var cellControlType = CellStaticData.GetCellControlType(cell.GetType());
            var constructor = GetCellControlConstructor(cell.GetType(), new object[] {cell, tableControl});
            if (constructor != null)
            {
                CellControl result = (CellControl)constructor.Invoke(new object[] { cell, tableControl });
                result.OnCreationComplete();
                return result;
            }

            Debug.LogError($"{cellControlType.Name} lacks required constructor.");
            return null;
        }
        
        private static ConstructorInfo GetCellControlConstructor(Type cellType, object[] args)
        {
            if (_cellControlConstructors.TryGetValue(cellType, out var controlConstructor))
                return controlConstructor;
            
            var cellControlType = CellStaticData.GetCellControlType(cellType);
            var constructor = cellControlType.GetConstructor(args.Select(x => x.GetType()).ToArray());
            _cellControlConstructors.Add(cellType, constructor);
            return constructor;
        }    
    }
}