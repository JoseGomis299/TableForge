namespace TableForge.UI
{
    internal static class TableControlExtensions
    {
        public static CellAnchor GetRowAtPosition(this TableControl tableControl, int position)
        {
            if (!tableControl.Inverted)
            {
                if (tableControl.TableData.Rows.TryGetValue(position, out var row))
                    return row;
            }
            else
            {
                if (tableControl.TableData.Columns.TryGetValue(position, out var column))
                    return column;
            }

            return null;
        }
        
        public static CellAnchor GetColumnAtPosition(this TableControl tableControl, int position)
        {
            if (!tableControl.Inverted)
            {
                if (tableControl.TableData.Columns.TryGetValue(position, out var column))
                    return column;
            }
            else
            {
                if (tableControl.TableData.Rows.TryGetValue(position, out var row))
                    return row;
            }

            return null;
        }

        public static CellAnchor GetCellRow(this TableControl tableControl, Cell cell)
        {
            return !tableControl.Inverted ? cell.Row : cell.Column;
        }
        
        public static CellAnchor GetCellColumn(this TableControl tableControl, Cell cell)
        {
            return !tableControl.Inverted ? cell.Column : cell.Row;
        }

        public static int GetColumnPosition(this TableControl tableControl, int columnId)
        {
            if (!tableControl.ColumnData.ContainsKey(columnId))
                return -1;

            return tableControl.ColumnData[columnId].Position;
        }
        
        public static RowHeaderControl GetRowHeaderControl(this TableControl tableControl, CellAnchor row)
        {
            return tableControl.RowHeaders[row.Id];
        }
        
        public static ColumnHeaderControl GetColumnHeaderControl(this TableControl tableControl, CellAnchor column)
        {
            return tableControl.ColumnHeaders[column.Id];
        }
        
        public static TableControl GetRootTableControl(this TableControl tableControl)
        {
            return tableControl.Parent == null ? tableControl : tableControl.Parent.TableControl.GetRootTableControl();
        }

        public static int GetId(this TableControl tableControl)
        {
           return tableControl.TableData.IsSubTable ? tableControl.TableData.ParentCell.Id : 0;
        }
        
        public static Cell GetCell(this TableControl tableControl, int rowId, int columnId)
        {
            if (!tableControl.RowData.ContainsKey(rowId) || !tableControl.ColumnData.ContainsKey(columnId))
                return null;

            if (tableControl.RowData[rowId].CellAnchor is Row row)
            {
                if (row.Cells.TryGetValue(tableControl.ColumnData[columnId].Position, out var cell))
                    return cell;
            }
            else if (tableControl.ColumnData[columnId].CellAnchor is Row column)
            {
                if (column.Cells.TryGetValue(tableControl.RowData[rowId].CellAnchor.Position, out var cell))
                    return cell;
            }

            return null;
        }

        public static CellControl GetCellControl(this TableControl tableControl, int rowId, int columnId)
        {
            Cell cell = tableControl.GetCell(rowId, columnId);
            return CellControlFactory.GetCellControlFromId(cell.Id);
        }
        
        
        
    }
}