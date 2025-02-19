using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TableForge.UI
{
    internal static class SizeCalculator
    {
        #region Public Methods

        public static Vector2 CalculateSize(CellControl cellControl)
        {
            if (cellControl is SubTableCellControl subTableCell)
            {
                return CalculateSize(subTableCell);
            }

            CellSizeCalculationMethod method = 
                CellStaticData.GetCellAttributes(cellControl.GetType()).SizeCalculationMethod;

            return method switch
            {
                CellSizeCalculationMethod.FixedBigCell => new Vector2(UiConstants.BigCellDesiredWidth,
                    UiConstants.CellHeight),
                CellSizeCalculationMethod.FixedRegularCell =>
                    new Vector2(UiConstants.CellWidth, UiConstants.CellHeight),
                CellSizeCalculationMethod.FixedSmallCell => new Vector2(UiConstants.SmallCellDesiredWidth,
                    UiConstants.CellHeight),
                CellSizeCalculationMethod.AutoSize => CalculateAutoSize(cellControl),
                _ => new Vector2(UiConstants.CellWidth, UiConstants.CellHeight)
            };
        }
        
        public static Vector2 CalculateSize(Cell cell, int page)
        {
            if (cell is SubTableCell subTableCell)
            {
                return CalculateSize(subTableCell, page);
            }

            CellSizeCalculationMethod method = 
                CellStaticData.GetCellAttributes(CellStaticData.GetCellControlType(cell.GetType())).SizeCalculationMethod;

            return method switch
            {
                CellSizeCalculationMethod.FixedBigCell => new Vector2(UiConstants.BigCellDesiredWidth,
                    UiConstants.CellHeight),
                CellSizeCalculationMethod.FixedRegularCell =>
                    new Vector2(UiConstants.CellWidth, UiConstants.CellHeight),
                CellSizeCalculationMethod.FixedSmallCell => new Vector2(UiConstants.SmallCellDesiredWidth,
                    UiConstants.CellHeight),
                CellSizeCalculationMethod.AutoSize => CalculateAutoSize(cell),
                _ => new Vector2(UiConstants.CellWidth, UiConstants.CellHeight)
            };
        }

        public static Vector2 CalculateSize(TableControl tableControl)
        {
            Table table = tableControl.TableData;
            List<Row> rows = table.Rows.Values.Where(row => row.Position >= tableControl.PageManager.FirstRowPosition && row.Position <= tableControl.PageManager.LastRowPosition).ToList();
            List<Vector2> cellSizes = new List<Vector2>();
            
            foreach (var row in  rows)
            {
                foreach (var cell in row.Cells.Values)
                {
                    Vector2 size;
                    if (cell is SubTableCell && tableControl.GetCell(cell.Row.Id, cell.Column.Id) is SubTableCellControl subCell)
                    {
                       size = CalculateSize(subCell);
                    }
                    else size = CalculateSize(cell, 0);
                    
                    cellSizes.Add(size + Vector2.one * UiConstants.BorderWidth);
                }
            }
            
            return CalculateSize(tableControl.TableData, tableControl.TableAttributes, tableControl.PageManager.Page, cellSizes, tableControl.VisibleColumns);
        }
        
        public static Vector2 CalculateSize(Table table, TableAttributes tableAttributes, int page)
        {
            List<Row> rows = table.Rows.Values.Where(row => row.Position >= TablePageManager.GetFirstRowPosition(page, table.Rows.Count) && row.Position <= TablePageManager.GetLastRowPosition(page, table.Rows.Count)).ToList();
            List<Vector2> cellSizes = new List<Vector2>();
            
            foreach (var row in  rows)
            {
                foreach (var cell in row.Cells.Values)
                {
                    cellSizes.Add(CalculateSize(cell, 0) + Vector2.one * UiConstants.BorderWidth);
                }
            }

            return CalculateSize(table, tableAttributes, page, cellSizes, Enumerable.Repeat(true, table.Columns.Count).ToArray());
        }
        
        public static Vector2 CalculateHeaderSize(CellAnchor header, TableHeaderVisibility visibility)
        {
            if (visibility == TableHeaderVisibility.Hidden)
                return Vector2.zero;
            
            string headerName = HeaderNameResolver.ResolveHeaderName(header, visibility);
            return new Vector2(Mathf.Max(EditorStyles.label.CalcSize(new GUIContent(headerName)).x, UiConstants.MinCellWidth) + UiConstants.HeaderPadding, UiConstants.CellHeight + UiConstants.HeaderPadding);
        }

        #endregion
        
        #region Private Methods
        
        private static Vector2 CalculateSize(Table table, TableAttributes tableAttributes, int page, List<Vector2> cellSizes, bool[] visibleColumns)
        {
            float width = UiConstants.CellContentPadding / 2f, height = UiConstants.CellContentPadding / 2f;

            int pageSize = ToolbarData.PageSize;
            
            bool hasRowHeaders = tableAttributes.RowHeaderVisibility != TableHeaderVisibility.Hidden;
            bool hasColumnHeaders = tableAttributes.ColumnHeaderVisibility != TableHeaderVisibility.Hidden;
            
            List<Row> rows = table.Rows.Values.Where(row => row.Position >= (page - 1)*pageSize && row.Position <= pageSize).ToList();
            
            if(rows.Count == 0 || pageSize == 0)
            {
                //Empty table
                if((table.Columns.Count == 0 || !visibleColumns.Any()) && !hasColumnHeaders)
                    return new Vector2(UiConstants.MinCellWidth, UiConstants.MinCellHeight);
                
                if(hasRowHeaders)
                    width += UiConstants.CellWidth; //Corner cell
                
                foreach (var column in table.Columns.Values)
                {
                    if(!visibleColumns[column.Position - 1]) continue;
                    width += CalculateHeaderSize(column, tableAttributes.ColumnHeaderVisibility).x;
                }
                
                //The table has no rows, but has columns, so we need to return the width of the headers
                return new Vector2(width, UiConstants.CellHeight + UiConstants.HeaderPadding);
            }
            
            //Add the row headers width
            if(hasRowHeaders)
                width += rows
                    .Select(row => CalculateHeaderSize(row, tableAttributes.RowHeaderVisibility).x).Max();
            
            //If there are no columns the table will be just the row headers or empty if there are no row headers
            if (table.Columns.Count == 0 || !visibleColumns.Any())
            {
                width = hasRowHeaders ? width : UiConstants.MinCellWidth;
                height = hasRowHeaders ? UiConstants.CellHeight * rows.Count : UiConstants.MinCellHeight;
                
                return new Vector2(width, height);
            }

            //Calculate the table height
            for (int i = 0; i < rows.Count; i++)
            {
                float maxRowHeight = UiConstants.CellHeight;
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    if(!visibleColumns[j]) continue;
                    maxRowHeight = Mathf.Max(maxRowHeight, cellSizes[i * table.Columns.Count + j].y);
                }

                height += maxRowHeight;
            }

            if(hasColumnHeaders)
                height += UiConstants.CellHeight; //Add the header row height
            
            //Add columns width
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if(!visibleColumns[i]) continue;
                float maxColumnWidth = CalculateHeaderSize(table.Columns[i + 1], tableAttributes.ColumnHeaderVisibility).x;
                for (int j = 0; j < rows.Count; j++)
                {
                    maxColumnWidth = Mathf.Max(maxColumnWidth, cellSizes[j*table.Columns.Count + i].x);
                }
                
                width += maxColumnWidth;
            }

            return new Vector2(width, height);
        }
        
        private static Vector2 CalculateAutoSize(CellControl cell)
        {
            float width = cell.Cell.GetValue() == null ? UiConstants.SmallCellDesiredWidth : EditorStyles.label.CalcSize(new GUIContent(cell.Cell.GetValue().ToString())).x;
            if(width < UiConstants.SmallCellDesiredWidth)
                width = UiConstants.SmallCellDesiredWidth;
            
            return new Vector2(width + UiConstants.CellContentPadding, UiConstants.CellHeight + UiConstants.CellContentPadding);
        }
        
        private static Vector2 CalculateAutoSize(Cell cell)
        {
            float width = cell.GetValue() == null ? UiConstants.SmallCellDesiredWidth : EditorStyles.label.CalcSize(new GUIContent(cell.GetValue().ToString())).x;
            if(width < UiConstants.SmallCellDesiredWidth)
                width = UiConstants.SmallCellDesiredWidth;
            
            return new Vector2(width + UiConstants.CellContentPadding, UiConstants.CellHeight + UiConstants.CellContentPadding);
        }
        
        private static Vector2 CalculateSize(SubTableCell subTableCell, int page)
        {
            float width = UiConstants.CellContentPadding, height = UiConstants.CellContentPadding;
            Type cellControlType = CellStaticData.GetCellControlType(subTableCell.GetType()); 
            TableAttributes subTableAttributes = CellStaticData.GetSubTableCellAttributes(cellControlType);
           
            Vector2 tableSize = CalculateSize(subTableCell.SubTable, subTableAttributes, page);
            return new Vector2(width + tableSize.x, height + tableSize.y);
        }
        
        private static Vector2 CalculateSize(SubTableCellControl subTableCellControl)
        {
            float width = UiConstants.CellContentPadding, height = UiConstants.CellContentPadding;

            Vector2 tableSize = subTableCellControl.SubTableControl != null ?
                CalculateSize(subTableCellControl.SubTableControl) 
                : CalculateSize(subTableCellControl.Cell, 0);
            
            return new Vector2(width + tableSize.x, height + tableSize.y);
        }
        
        #endregion
    }
}