using System;
using System.Collections.Generic;
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
                CellSizeCalculationMethod.FixedBigCell => new Vector2(UiConstants.BigCellPreferredWidth,
                    UiConstants.CellHeight),
                CellSizeCalculationMethod.FixedRegularCell =>
                    new Vector2(UiConstants.CellWidth, UiConstants.CellHeight),
                CellSizeCalculationMethod.FixedSmallCell => new Vector2(UiConstants.SmallCellPreferredWidth,
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
            Vector2 size = Vector2.zero;
            if (tableControl.Parent is ExpandableSubTableCellControl { IsFoldoutOpen: false })
            {
                size.y = UiConstants.FoldoutHeight;
                size.x = EditorStyles.foldoutHeader.CalcSize(new GUIContent(tableControl.Parent.Cell.Column.Name)).x +
                         EditorStyles.foldoutHeaderIcon.fixedWidth;
            }
            else
            {
                TableSize tableSize = tableControl.PreferredSize;
                size = tableSize.GetTotalSize(true);
                size.y += UiConstants.CellContentPadding + UiConstants.BorderWidth * 5;
                
                if (tableControl.Parent is ExpandableSubTableCellControl)
                {
                    size.x += UiConstants.SubTableToolbarWidth;
                }
            }
            
            Vector2 clampedSize = GetClampedSize(size + Vector2.right * UiConstants.CellContentPadding);
            return clampedSize;
        }
        
        public static TableSize CalculateTableSize(Table table, TableAttributes tableAttributes, TableMetadata tableMetadata)
        {
            TableSize tableSize = new TableSize(table, tableMetadata, tableAttributes);
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
                if (!tableMetadata.IsFieldVisible(column.Id)) continue;

                tableSize.AddHeaderSize(column, CalculateHeaderSize(column, tableAttributes.ColumnHeaderVisibility));
            }
            
            return tableSize;
        }

        #endregion
        
        #region Private Methods

        private static Vector2 CalculateHeaderSize(CellAnchor header, TableHeaderVisibility visibility)
        {
            if (visibility == TableHeaderVisibility.Hidden)
                return Vector2.zero;

            float padding = visibility is TableHeaderVisibility.ShowHeaderNumber or TableHeaderVisibility.ShowHeaderLetter or TableHeaderVisibility.ShowHeaderNumberBase0 ? 
                UiConstants.SmallHeaderPadding 
                : UiConstants.HeaderPadding;
            
            if (header == null)
                return new Vector2(UiConstants.MinCellWidth, UiConstants.HeaderHeight + padding);
            
            string headerName = NameResolver.ResolveHeaderName(header, visibility);
            return new Vector2(Mathf.Max(EditorStyles.label.CalcSize(new GUIContent(headerName)).x, UiConstants.MinCellWidth) + padding, UiConstants.HeaderHeight);
        }
        
        private static float GetAddRowButtonHeight(Table table, TableAttributes tableAttributes)
        {
            if (tableAttributes.TableType == TableType.Dynamic
                || (tableAttributes.TableType == TableType.DynamicIfEmpty && table.Rows.Count == 0))
                return UiConstants.CellHeight;
            
            return 0;
        }
        
        private static Vector2 CalculateAutoSize(Cell cell)
        {
            float width = cell.GetValue() == null ? UiConstants.SmallCellPreferredWidth : EditorStyles.label.CalcSize(new GUIContent(cell.GetValue().ToString())).x;
            if(width < UiConstants.SmallCellPreferredWidth)
                width = UiConstants.SmallCellPreferredWidth;
            
            return new Vector2(width + UiConstants.CellContentPadding, UiConstants.CellHeight);
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
            float width = UiConstants.CellContentPadding + UiConstants.BorderWidth * 4, height = UiConstants.BorderWidth * 4;
            Type cellControlType = CellStaticData.GetCellControlType(subTableCell.GetType()); 
            TableAttributes subTableAttributes = CellStaticData.GetSubTableCellAttributes(cellControlType);
            
            Vector2 localSize = Vector2.zero;

            if(parentMetadata.IsTableExpanded(subTableCell.Id))
            {
                var tableSize = CalculateTableSize(subTableCell.SubTable, subTableAttributes, parentMetadata);
                localSize = tableSize.GetTotalSize(false);
                localSize.x += UiConstants.SubTableToolbarWidth;
                localSize.y += UiConstants.CellContentPadding;
            }
            else
            {
                localSize.y = UiConstants.FoldoutHeight;
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