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
        
        public static Vector2 CalculateSize(Cell cell, TableMetadata tableMetadata)
        {
            Vector2 size;
            if (cell is SubTableCell subTableCell)
            {
                size = CalculateSize(subTableCell, tableMetadata);
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
        
        public static Vector2 CalculateSizeWithCurrentCellSizes(TableControl tableControl)
        {
            TableSize tableSize = tableControl.TableSize;
            
            foreach (var row in  tableControl.RowHeaders.Values)
            {
                tableSize.AddHeaderSize(row.CellAnchor, new Vector2(row.style.width.value.value, row.style.height.value.value));
            }
            
            foreach (var column in tableControl.ColumnHeaders.Values)
            {
                tableSize.AddHeaderSize(column.CellAnchor, new Vector2(column.style.width.value.value, UiConstants.CellHeight));
            }
            
            return GetClampedSize(tableSize.GetTotalSize(false) + Vector2.one * (UiConstants.CellContentPadding * 1.5f + UiConstants.BorderWidth));
        }
        
        public static TableSize CalculateSize(Table table, TableAttributes tableAttributes, TableMetadata tableMetadata)
        {
            TableSize tableSize = new TableSize(table, tableAttributes);
            IReadOnlyList<Row> rows = table.OrderedRows;

            foreach (var row in  rows)
            {
                foreach (var cell in row.Cells.Values)
                {
                    Vector2 cellSize = CalculateSize(cell, tableMetadata) + Vector2.one * UiConstants.BorderWidth;
                    tableSize.AddCellSize(cell, cellSize);
                }
                
                tableSize.AddHeaderSize(row, CalculateHeaderSize(row, tableAttributes.RowHeaderVisibility));
            }
            
            tableSize.AddHeaderSize(null, CalculateHeaderSize(null, tableAttributes.RowHeaderVisibility));
            
            for (int i = 0; i < table.Columns.Count; i++)
            {
                Column column = table.Columns[i + 1];
                if (!tableMetadata.VisibleFields.Contains(column.Id) && tableMetadata.Name == table.Name) continue;

                tableSize.AddHeaderSize(column, CalculateHeaderSize(column, tableAttributes.ColumnHeaderVisibility));
            }

            return tableSize;
        }
        
        public static Vector2 CalculateHeaderSize(CellAnchor header, TableHeaderVisibility visibility)
        {
            if (visibility == TableHeaderVisibility.Hidden)
                return Vector2.zero;

            if (header == null)
                return new Vector2(UiConstants.MinCellWidth, UiConstants.MinCellHeight);
            
            string headerName = NameResolver.ResolveHeaderName(header, visibility);
            float padding = visibility is TableHeaderVisibility.ShowHeaderNumber or TableHeaderVisibility.ShowHeaderLetter or TableHeaderVisibility.ShowHeaderNumberBase0 ? 
                UiConstants.SmallHeaderPadding 
                : UiConstants.HeaderPadding;
            
            return new Vector2(Mathf.Max(EditorStyles.label.CalcSize(new GUIContent(headerName)).x, UiConstants.MinCellWidth) + padding, UiConstants.CellHeight + padding);
        }

        #endregion
        
        #region Private Methods
        
        private static Vector2 CalculateSize(Table table, TableAttributes tableAttributes, List<Vector2> cellSizes, List<int> visibleColumns)
        {
            float width = UiConstants.CellContentPadding / 2f, height = UiConstants.CellContentPadding / 2f;
            
            if(tableAttributes.TableType == TableType.Dynamic || table.Rows.Count == 0)
                height += UiConstants.CellHeight; //Add the AddRowButton height
            
            bool hasRowHeaders = tableAttributes.RowHeaderVisibility != TableHeaderVisibility.Hidden;
            bool hasColumnHeaders = tableAttributes.ColumnHeaderVisibility != TableHeaderVisibility.Hidden;

            IReadOnlyList<Row> rows = table.OrderedRows;
            
            if(rows.Count == 0)
            {
                //Empty table
                if((table.Columns.Count == 0 || !visibleColumns.Any()) && !hasColumnHeaders)
                    return new Vector2(UiConstants.MinCellWidth, UiConstants.MinCellHeight);
                
                if(hasRowHeaders)
                    width += UiConstants.CellWidth; //Corner cell
                
                foreach (var column in table.Columns.Values)
                {
                    if(!visibleColumns.Contains(column.Id)) continue;
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
                    if(!visibleColumns.Contains(table.Columns[j+1].Id)) continue;
                    maxRowHeight = Mathf.Max(maxRowHeight, cellSizes[i * table.Columns.Count + j].y);
                }

                height += maxRowHeight;
            }

            if(hasColumnHeaders)
                height += UiConstants.CellHeight; //Add the header row height
            
            //Add columns width
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if(!visibleColumns.Contains(table.Columns[i+1].Id)) continue;
                float maxColumnWidth = CalculateHeaderSize(table.Columns[i + 1], tableAttributes.ColumnHeaderVisibility).x;
                for (int j = 0; j < rows.Count; j++)
                {
                    maxColumnWidth = Mathf.Max(maxColumnWidth, cellSizes[j*table.Columns.Count + i].x);
                }
                
                width += maxColumnWidth;
            }

            return new Vector2(width, height);
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
        
        private static Vector2 CalculateSize(SubTableCell subTableCell, TableMetadata parentMetadata)
        {
            float width = UiConstants.CellContentPadding * 1.5f, height = UiConstants.CellContentPadding + UiConstants.FoldoutHeight;
            Type cellControlType = CellStaticData.GetCellControlType(subTableCell.GetType()); 
            TableAttributes subTableAttributes = CellStaticData.GetSubTableCellAttributes(cellControlType);
            
            Vector2 localSize = Vector2.zero;

            if(parentMetadata.CellMetadata.TryGetValue(subTableCell.Id, out var cellMetadata))
            {
                if (!cellMetadata.isExpanded)
                {
                    localSize.y = 0;
                    localSize.x = EditorStyles.foldoutHeader.CalcSize(new GUIContent(subTableCell.Column.Name)).x +
                                  EditorStyles.foldoutHeaderIcon.fixedWidth;
                }
                else
                {
                    var tableSize = CalculateSize(subTableCell.SubTable, subTableAttributes, parentMetadata);
                    localSize = tableSize.GetTotalSize(false);
                }
            }
            else
            {
                localSize.y = 0;
                localSize.x = EditorStyles.foldoutHeader.CalcSize(new GUIContent(subTableCell.Column.Name)).x +
                              EditorStyles.foldoutHeaderIcon.fixedWidth;
            }

            return new Vector2(width + localSize.x, height + localSize.y);
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