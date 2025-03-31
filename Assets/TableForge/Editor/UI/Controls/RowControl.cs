using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;


namespace TableForge.UI
{
    internal class RowControl : VisualElement
    {
        public CellAnchor Anchor { get; }
        private TableControl TableControl { get; }
        private float _offset;
        
        public RowControl(CellAnchor anchor, TableControl tableControl)
        {
            TableControl = tableControl;
            Anchor = anchor;
            
            AddToClassList(USSClasses.TableRow);
           // AddToClassList(USSClasses.Hidden);
        }
        
        public void ClearRow()
        {
            foreach (var child in Children())
            {
                if (child is CellControl cell)
                    CellControlFactory.Release(cell);
            }
            
            Clear();
        }

        public void RefreshColumnWidths()
        {
            if(!Children().Any()) return;
            
            foreach (var child in Children())
            {
                if (child is CellControl cellControl)
                {
                    var column = TableControl.GetColumnHeaderControl(TableControl.GetCellColumn(cellControl.Cell));
                    cellControl.style.width = column.style.width;
                }
            }
        }
        
        public void Refresh()
        {
            foreach (var child in Children())
            {
                if (child is CellControl cell)
                    cell.Refresh();
            }
        }

        public void Refresh(CellAnchor anchor)
        {
            ClearRow();
            
            if (anchor is Row row) InitializeRow(row);
            else InitializeRow(anchor);
            
            RefreshColumnWidths();
        }

        private void InitializeRow(Row row)
        {
            var columnsByPosition = TableControl.ColumnData.ToDictionary(c => c.Value.Position, c => c.Value);
            columnsByPosition = columnsByPosition.OrderBy(c => c.Key).ToDictionary(c => c.Key, c => c.Value);
                        
            foreach (var columnEntry in columnsByPosition)
            {
                if (!row.Cells.TryGetValue(columnEntry.Key, out var cell) || !TableControl.ColumnHeaders[columnEntry.Value.Id].IsVisible) continue;

                var cellField = CreateCellField(cell);
                Add(cellField);
            }
        }
        
        private void InitializeRow(CellAnchor column)
        {
            var orderedRows = TableControl.TableData.OrderedRows;

            foreach (var row in orderedRows)
            {
                if (!row.Cells.TryGetValue(column.Position, out var cell)  || !TableControl.ColumnHeaders[row.Id].IsVisible) continue;

                Debug.Log("VAR");
                var cellField = CreateCellField(cell);
                Add(cellField);
            }
        }

        private VisualElement CreateCellField(Cell cell)
        {
            if(cell == null) return new Label {text = ""};
            var cellControl = CellControlFactory.Create(cell, TableControl);
            return cellControl;
        }


        public void SetColumnVisibility(int columnId, bool isVisible, int direction)
        {
            int columnPosition = TableControl.GetColumnPosition(columnId);
            
            Cell cell = TableControl.GetCell(Anchor.Id, columnId);
            
            if (isVisible)
            {
                var cellField = CreateCellField(cell);
                
                if(direction == 1)
                    Add(cellField);
                else
                    Insert(0, cellField);
            }
            else
            {
                var cellControl = Children().OfType<CellControl>().FirstOrDefault(c => TableControl.GetCellColumn(c.Cell).Position == columnPosition);
                
                if (cellControl != null)
                {
                    CellControlFactory.Release(cellControl);
                    Remove(cellControl);
                }
            }
            
            RefreshColumnWidths();
        }
    }
}