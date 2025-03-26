using System;
using System.Collections.Generic;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class CellControlPool
    {
        private Dictionary<Type, ObjectPool<CellControl>> _cellControlPools = new Dictionary<Type, ObjectPool<CellControl>>();
        private Dictionary<Type, Dictionary<int, ObjectPool<CellControl>>> _subTableCellControlPools = new Dictionary<Type, Dictionary<int, ObjectPool<CellControl>>>();
        
        private Cell _cell;
        private TableControl _tableControl;

        public CellControl GetCellControl(Cell cell, TableControl tableControl)
        {
            _cell = cell;
            _tableControl = tableControl;
            
            if (cell is SubTableCell subTableCell)
            {
                return GetSubTableCellControl(subTableCell, tableControl);
            }
            
            if(_cellControlPools.TryGetValue(cell.Type, out var pool))
            {
                var cellControl = pool.Get();
                return cellControl;
            }

            pool = new ObjectPool<CellControl>(CreateCellControl, _ => { }, _ => {});
            _cellControlPools.Add(cell.Type, pool);
            var control = pool.Get();
            return control;
        }
        
        public void Release(CellControl cellControl)
        {
            if(_cellControlPools.TryGetValue(cellControl.Cell.Type, out var pool))
            {
                pool.Release(cellControl);
            }
            else if (_subTableCellControlPools.TryGetValue(cellControl.Cell.Type, out var subTableCellPools))
            {
                if (subTableCellPools.TryGetValue(((SubTableCell)cellControl.Cell).SubTable.Rows.Count, out var subTableCellPool))
                {
                    ((SubTableCellControl)cellControl).SubTableControl?.ClearTable();
                    subTableCellPool.Release(cellControl);
                }
            }
        }
        
        private CellControl GetSubTableCellControl(SubTableCell subTableCell, TableControl tableControl)
        {
            if (!_subTableCellControlPools.TryGetValue(subTableCell.Type, out var subTableCellPools))
            {
                subTableCellPools = new Dictionary<int, ObjectPool<CellControl>>();
                _subTableCellControlPools.Add(subTableCell.Type, subTableCellPools);
            }

            if (!subTableCellPools.TryGetValue(subTableCell.SubTable.Rows.Count, out var pool))
            {
                pool = new ObjectPool<CellControl>(CreateCellControl, _ => { }, _ => {});
                subTableCellPools.Add(subTableCell.SubTable.Rows.Count, pool);
            }

            var control = pool.Get();
            return control;
        }
        
        private CellControl CreateCellControl()
        {
            return CellControlFactory.CreateCellControl(_cell, _tableControl);
        }
    }
}