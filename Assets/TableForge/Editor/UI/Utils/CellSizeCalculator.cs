using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TableForge.UI
{
    internal static class CellSizeCalculator
    {
        public static Vector2 CalculateSize(Cell cell)
        {
            if (cell is SubTableCell subTableCell)
            {
                return CalculateSize(subTableCell);
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
        
        private static Vector2 CalculateSize(SubTableCell subTableCell)
        {
            float width = 18, height = 18;
            Type cellControlType = CellStaticData.GetCellControlType(subTableCell.GetType()); 
            TableAttributes subTableAttributes = CellStaticData.GetSubTableCellAttributes(cellControlType);
            bool hasRowHeaders = subTableAttributes.RowHeaderVisibility != TableHeaderVisibility.Hidden;
            bool hasColumnHeaders = subTableAttributes.ColumnHeaderVisibility != TableHeaderVisibility.Hidden;
            
            if(subTableCell.SubTable.Rows.Count == 0)
            {
                //Empty table
                if(subTableCell.SubTable.Columns.Count == 0 && !hasColumnHeaders)
                    return new Vector2(UiConstants.MinCellWidth, UiConstants.MinCellHeight);
                
                if(hasRowHeaders)
                    width += UiConstants.CellWidth; //Corner cell
                
                foreach (var column in subTableCell.SubTable.Columns.Values)
                {
                    width += CalculateHeaderSize(column, subTableAttributes.ColumnHeaderVisibility).x;
                }
                
                //The table has no rows, but has columns, so we need to return the width of the headers
                return new Vector2(width, UiConstants.CellHeight);
            }
            
            //Add the row headers width
            width += subTableCell.SubTable.Rows.Values
                .Select(row => CalculateHeaderSize(row, subTableAttributes.RowHeaderVisibility).x).Max();
            
            //If there are no columns the table will be just the row headers or empty if there are no row headers
            if (subTableCell.SubTable.Columns.Count == 0)
            {
                width = hasRowHeaders ? width : UiConstants.MinCellWidth;
                height = hasRowHeaders ? UiConstants.CellHeight * subTableCell.SubTable.Rows.Count : UiConstants.MinCellHeight;
                
                return new Vector2(width, height);
            }

            //Calculate the table height
            foreach (var row in  subTableCell.SubTable.Rows.Values)
            {
                float maxRowHeight = UiConstants.CellHeight;
                foreach (var cell in row.Cells.Values)
                {
                    Vector2 size = CalculateSize(cell);
                    maxRowHeight = Mathf.Max(maxRowHeight, size.y);
                }

                height += maxRowHeight + UiConstants.CellContentPadding;
            }
            if(hasColumnHeaders)
                height += UiConstants.CellHeight; //Add the header row height
            
            //Add columns width
            foreach (var column in subTableCell.SubTable.Columns.Values)
            {
                float maxColumnWidth = CalculateHeaderSize(column, subTableAttributes.ColumnHeaderVisibility).x;
                foreach (var row in  subTableCell.SubTable.Rows.Values)
                {
                    if (row.Cells.TryGetValue(column.Position, out Cell cell))
                    {
                        Vector2 size = CalculateSize(cell);
                        maxColumnWidth = Mathf.Max(maxColumnWidth, size.x);
                    }
                }

                width += maxColumnWidth + UiConstants.CellContentPadding;
            }

            Debug.Log($"{subTableCell.Column.Name} from row {subTableCell.Row.Name} has size {width}x{height}");
            return new Vector2(width, height);
        }
        
        private static Vector2 CalculateAutoSize(Cell cell)
        {
            float width = cell.GetValue() == null ? UiConstants.SmallCellDesiredWidth : EditorStyles.label.CalcSize(new GUIContent(cell.GetValue().ToString())).x;
            if(width < UiConstants.SmallCellDesiredWidth)
                width = UiConstants.SmallCellDesiredWidth;
            
            return new Vector2(width + UiConstants.CellContentPadding, UiConstants.CellHeight + UiConstants.CellContentPadding);
        }

        public static Vector2 CalculateHeaderSize(CellAnchor header, TableHeaderVisibility visibility)
        {
            if (visibility == TableHeaderVisibility.Hidden)
                return Vector2.zero;
            
            string headerName = HeaderNameResolver.ResolveHeaderName(header, visibility);
            return new Vector2(EditorStyles.label.CalcSize(new GUIContent(headerName)).x + UiConstants.HeaderPadding, UiConstants.CellHeight);
        }
    }
}