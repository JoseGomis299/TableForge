using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TableForge.UI
{
    internal static class SizeCalculator
    {
        #region Public Methods

        public static Vector2 CalculateSize(CellControl cellControl)
        {
            Vector2 size;
            if (cellControl is SubTableCellControl subTableCell)
            {
                size = CalculateSize(subTableCell);
                return GetClampedSize(size);
            }

            CellSizeCalculationMethod method = 
                CellStaticData.GetCellAttributes(cellControl.GetType()).SizeCalculationMethod;

            size = method switch
            {
                CellSizeCalculationMethod.FixedBigCell => new Vector2(UiConstants.BigCellDesiredWidth,
                    UiConstants.CellHeight),
                CellSizeCalculationMethod.FixedRegularCell =>
                    new Vector2(UiConstants.CellWidth, UiConstants.CellHeight),
                CellSizeCalculationMethod.FixedSmallCell => new Vector2(UiConstants.SmallCellDesiredWidth,
                    UiConstants.CellHeight),
                CellSizeCalculationMethod.AutoSize => CalculateAutoSize(cellControl),
                CellSizeCalculationMethod.EnumAutoSize => CalculateEnumAutoSize(cellControl),
                CellSizeCalculationMethod.ReferenceAutoSize => CalculateReferenceAutoSize(cellControl),
                _ => new Vector2(UiConstants.CellWidth, UiConstants.CellHeight)
            };
            
            return GetClampedSize(size);
        }
        
        public static Vector2 CalculateSize(Cell cell, int page)
        {
            Vector2 size;
            if (cell is SubTableCell subTableCell)
            {
                size = CalculateSize(subTableCell, page);
                return GetClampedSize(size);
            }

            CellSizeCalculationMethod method = 
                CellStaticData.GetCellAttributes(CellStaticData.GetCellControlType(cell.GetType())).SizeCalculationMethod;

            size = method switch
            {
                CellSizeCalculationMethod.FixedBigCell => new Vector2(UiConstants.BigCellDesiredWidth,
                    UiConstants.CellHeight),
                CellSizeCalculationMethod.FixedRegularCell =>
                    new Vector2(UiConstants.CellWidth, UiConstants.CellHeight),
                CellSizeCalculationMethod.FixedSmallCell => new Vector2(UiConstants.SmallCellDesiredWidth,
                    UiConstants.CellHeight),
                CellSizeCalculationMethod.AutoSize => CalculateAutoSize(cell),
                CellSizeCalculationMethod.EnumAutoSize => CalculateEnumAutoSize(cell),
                CellSizeCalculationMethod.ReferenceAutoSize => CalculateReferenceAutoSize(cell),
                _ => new Vector2(UiConstants.CellWidth, UiConstants.CellHeight)
            };
            
            return GetClampedSize(size);
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
        
        public static Vector2 CalculateSizeWithCurrentCellSizes(TableControl tableControl)
        {
            List<Vector2> cellSizes = new List<Vector2>();
            List<ColumnHeaderControl> columnHeaders = tableControl.ColumnHeaders.Values.OrderBy(x => x.CellAnchor.Position).ToList();
            List<RowHeaderControl> rowHeaders = tableControl.RowHeaders.Values.OrderBy(x => x.CellAnchor.Position).ToList();
            
            foreach (var row in  rowHeaders)
            {
                foreach (var column in columnHeaders)
                {
                    cellSizes.Add(new Vector2(column.resolvedStyle.width, row.resolvedStyle.height));
                }
            }
            
            return GetClampedSize(CalculateSize(tableControl.TableData, tableControl.TableAttributes, tableControl.PageManager.Page, cellSizes, tableControl.VisibleColumns)
                                  + Vector2.one * (UiConstants.CellContentPadding + UiConstants.BorderWidth));
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
            
            string headerName = NameResolver.ResolveHeaderName(header, visibility);
            float padding = visibility is TableHeaderVisibility.ShowHeaderNumber or TableHeaderVisibility.ShowHeaderLetter or TableHeaderVisibility.ShowHeaderNumberBase0 ? 
                UiConstants.SmallHeaderPadding 
                : UiConstants.HeaderPadding;
            
            return new Vector2(Mathf.Max(EditorStyles.label.CalcSize(new GUIContent(headerName)).x, UiConstants.MinCellWidth) + padding, UiConstants.CellHeight + padding);
        }

        #endregion
        
        #region Private Methods
        
        private static Vector2 CalculateSize(Table table, TableAttributes tableAttributes, int page, List<Vector2> cellSizes, bool[] visibleColumns)
        {
            float width = UiConstants.CellContentPadding / 2f, height = UiConstants.CellContentPadding / 2f;
            
            if(tableAttributes.TableType == TableType.Dynamic || table.Rows.Count == 0)
                height += UiConstants.CellHeight; //Add the AddRowButton height
                
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
                float maxRowHeight = 0;
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
            return CalculateAutoSize(cell.Cell);
        }
        
        private static Vector2 CalculateEnumAutoSize(CellControl cell)
        {
            return CalculateEnumAutoSize(cell.Cell);
        }
        
        private static Vector2 CalculateReferenceAutoSize(CellControl cell)
        {
            return CalculateReferenceAutoSize(cell.Cell);
        }
        
        private static Vector2 CalculateAutoSize(Cell cell)
        {
            float width = cell.GetValue() == null ? UiConstants.SmallCellDesiredWidth : EditorStyles.label.CalcSize(new GUIContent(cell.GetValue().ToString())).x;
            if(width < UiConstants.SmallCellDesiredWidth)
                width = UiConstants.SmallCellDesiredWidth;
            
            return new Vector2(width + UiConstants.CellContentPadding, UiConstants.CellHeight + UiConstants.CellContentPadding);
        }
        
        private static Vector2 CalculateEnumAutoSize(Cell cell)
        {
            float padding = UiConstants.EnumArrowSize;
            string enumValue = cell.GetValue().ToString().ConvertToProperCase();
            
            if (cell is LayerMaskCell)
            {
                enumValue = NameResolver.ResolveLayerMaskName((LayerMask)cell.GetValue());
            }
            else if (cell is EnumCell && cell.Type.GetCustomAttribute<FlagsAttribute>() != null)
            {
                enumValue = NameResolver.ResolveFlagsEnumName(cell.Type, (int)cell.GetValue());
            }

            var preferredWidth = EditorStyles.popup.CalcSize(new GUIContent(enumValue)).x;
            return new Vector2(preferredWidth + padding, UiConstants.CellHeight);
        }
        
        private static Vector2 CalculateReferenceAutoSize(Cell cell)
        {
            var preferredWidth = cell.GetValue() as Object != null ? 
                EditorStyles.objectField.CalcSize(new GUIContent((cell.GetValue() as Object)?.name)).x :
                EditorStyles.objectField.CalcSize(new GUIContent($"None ({cell.Type.Name})")).x;
                 
            return new Vector2(preferredWidth + UiConstants.ReferenceTypeExtraSpace, UiConstants.CellHeight);
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
        
        private static Vector2 GetClampedSize(Vector2 size)
        {
            float width = 0, height = 0, additionalWidth = 0, additionalHeight = 0;

            if (size.x > UiConstants.MaxRecommendedWidth)
            {
                width = (int)UiConstants.MaxRecommendedWidth;
                additionalHeight = UiConstants.ScrollerWidth;
            }
            else if (size.x < UiConstants.MinCellWidth)
                width = (int) UiConstants.MinCellWidth;
            else
                width = (int) size.x;

            if (size.y > UiConstants.MaxRecommendedHeight)
            {
                height = (int)UiConstants.MaxRecommendedHeight;
                additionalWidth = UiConstants.ScrollerWidth;
            }
            else if (size.y < UiConstants.MinCellHeight)
                height = (int) UiConstants.MinCellHeight;
            else
                height = (int) size.y;
            
            return new Vector2(width + additionalWidth, height + additionalHeight);
        }
        
        #endregion
    }
}