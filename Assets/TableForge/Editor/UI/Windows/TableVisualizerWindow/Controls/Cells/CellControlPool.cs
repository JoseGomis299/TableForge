using System;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace TableForge.Editor.UI
{
    internal class CellControlPool
    {
        private readonly Dictionary<Type, ObjectPool<CellControl>> _cellControlPools = new();
        
        private Cell _cell;
        private TableControl _tableControl;

        public CellControl GetCellControl(Cell cell, TableControl tableControl)
        {
            _cell = cell;
            _tableControl = tableControl;

            if(!_cellControlPools.TryGetValue(cell.Type, out var pool))
            {
                pool = new ObjectPool<CellControl>(CreateCellControl);
                _cellControlPools.Add(cell.Type, pool);
            }

            var control = pool.Get();
            return control;
        }
        
        public void Release(CellControl cellControl)
        {
            if(_cellControlPools.TryGetValue(cellControl.Cell.Type, out var pool))
            {
                pool.Release(cellControl);
                
                if (cellControl is SubTableCellControl subTableCellControl)
                {
                    subTableCellControl.ClearSubTable();
                }
            }
        }
        
        private CellControl CreateCellControl()
        {
            return CellControlFactory.Create(_cell, _tableControl);
        }
    }
}