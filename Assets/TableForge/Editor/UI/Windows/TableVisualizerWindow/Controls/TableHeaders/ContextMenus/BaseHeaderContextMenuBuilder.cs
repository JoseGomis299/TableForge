using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    /// <summary>
    /// Base class for header context menu builders that provides common functionality.
    /// </summary>
    internal abstract class BaseHeaderContextMenuBuilder : IHeaderContextMenuBuilder
    {
        /// <summary>
        /// Builds the context menu for a header control.
        /// </summary>
        /// <param name="header">The header control for which to build the menu.</param>
        /// <param name="evt">The contextual menu populate event.</param>
        public abstract void BuildContextMenu(HeaderControl header, ContextualMenuPopulateEvent evt);

        /// <summary>
        /// Adds expand/collapse menu items for sub-table cells.
        /// </summary>
        /// <param name="header">The header control.</param>
        /// <param name="evt">The contextual menu populate event.</param>
        protected void AddExpandCollapseItems(HeaderControl header, ContextualMenuPopulateEvent evt)
        {
            List<SubTableCell> selectedCells = header.TableControl.CellSelector.GetSelectedCells(header.TableControl.TableData).OfType<SubTableCell>().ToList();
            bool containsSubTable = selectedCells.Any();
            
            if (!containsSubTable) return;
            
            evt.menu.AppendAction("Expand Selected", (_) =>
            {
                SetExpanded(header, selectedCells, true);
            });

            evt.menu.AppendAction("Collapse Selected", (_) =>
            {
                SetExpanded(header, selectedCells, false);
            });
        }
        
        /// <summary>
        /// Adds sorting menu items for column headers.
        /// </summary>
        /// <param name="header">The header control.</param>
        /// <param name="evt">The contextual menu populate event.</param>
        protected void AddSortingItems(HeaderControl header, ContextualMenuPopulateEvent evt)
        {
            if (header.CellAnchor is Column column)
            {
                evt.menu.AppendAction("Order Ascending", _ => header.TableControl.SortColumn(column, true));
                evt.menu.AppendAction("Order Descending", _ => header.TableControl.SortColumn(column, false));
            }
        }

        /// <summary>
        /// Sets the expanded state for a collection of sub-table cells.
        /// </summary>
        /// <param name="header">The header control.</param>
        /// <param name="cells">The cells to expand or collapse.</param>
        /// <param name="value">True to expand, false to collapse.</param>
        private void SetExpanded(HeaderControl header, IEnumerable<Cell> cells, bool value)
        {
            foreach (var cell in cells)
            {
                if(header.TableControl.Metadata.IsTableExpanded(cell.Id) == value) continue;
                        
                header.TableControl.Metadata.SetTableExpanded(cell.Id, value);
                header.TableControl.PreferredSize.AddCellSize(cell, SizeCalculator.CalculateSize(cell, header.TableControl.Metadata));
                header.TableControl.PreferredSize.StoreCellSizeInMetadata(cell);
            }
            
            header.TableControl.RebuildPage(false);
            if (header.TableControl.Parent != null)
            {
                foreach (var ancestor in header.TableControl.Parent.GetAncestors(true))
                {
                    ancestor.TableControl.PreferredSize.AddCellSize(ancestor.Cell, SizeCalculator.CalculateSize(ancestor.Cell, header.TableControl.Metadata));
                    ancestor.TableControl.PreferredSize.StoreCellSizeInMetadata(ancestor.Cell);
                    ancestor.TableControl.Resizer.ResizeAll(false);
                }
            }
        }
        
        /// <summary>
        /// Adds a "Create new asset" menu item to the context menu.
        /// </summary>
        /// <param name="headerControl">The header control.</param>
        /// <param name="evt">The contextual menu populate event.</param>
        protected void CreateItem(HeaderControl headerControl, ContextualMenuPopulateEvent evt)
        {
            if(headerControl.TableControl.Parent != null)
                return;
            
            evt.menu.AppendAction("Create new asset", _ =>
            {
                var tableControl = headerControl?.TableControl;
                if (tableControl?.Metadata == null) return;

                var itemsType = tableControl.Metadata.GetItemsType();
                if (itemsType == null || !typeof(ScriptableObject).IsAssignableFrom(itemsType))
                {
                    Debug.LogWarning("Can't create asset: table items type is null or not a ScriptableObject.");
                    return;
                }

                // Determine target folder
                string targetFolder = "Assets";
                if (headerControl is RowHeaderControl { CellAnchor: Row row })
                {
                    var rootObj = row.SerializedObject?.RootObject;
                    if (rootObj != null)
                    {
                        var p = AssetDatabase.GetAssetPath(rootObj);
                        if (!string.IsNullOrEmpty(p))
                        {
                            var dir = Path.GetDirectoryName(p)?.Replace('\\', '/');
                            if (!string.IsNullOrEmpty(dir) && AssetDatabase.IsValidFolder(dir))
                                targetFolder = dir;
                        }
                    }
                }
                else if (tableControl.TableData.Rows.Count > 0)
                { 
                    row = tableControl.TableData.Rows[1];
                    var rootObj = row.SerializedObject?.RootObject;
                    if (rootObj != null)
                    {
                        var p = AssetDatabase.GetAssetPath(rootObj);
                        if (!string.IsNullOrEmpty(p))
                        {
                            var dir = Path.GetDirectoryName(p)?.Replace('\\', '/');
                            if (!string.IsNullOrEmpty(dir) && AssetDatabase.IsValidFolder(dir))
                                targetFolder = dir;
                        }
                    }
                }

                // Create asset
                string defaultName = itemsType.Name + ".asset";
                string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{targetFolder}/{defaultName}");

                var newAsset = ScriptableObject.CreateInstance(itemsType);
                AssetDatabase.CreateAsset(newAsset, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                Selection.activeObject = newAsset;
                EditorGUIUtility.PingObject(newAsset);

                // Add to metadata if not type-bound
                if (!tableControl.Metadata.IsTypeBound)
                {
                    string guid = AssetDatabase.AssetPathToGUID(assetPath);
                    if (!string.IsNullOrEmpty(guid) && !tableControl.Metadata.ContainsItem(guid))
                        tableControl.Metadata.AddItemGuid(guid);
                }
                
                // Refresh table and select new row
                tableControl.CellSelector.ClearSelection();
                tableControl.RebuildPage(false);
                    
                if (tableControl.TableData.RowsByName.TryGetValue(newAsset.name, out row) && row.Cells.Count > 0)
                {
                        tableControl.CellSelector.SetSelection(row.OrderedCells.ToList());
                        tableControl.schedule.Execute(() =>
                        {
                            row.Cells[1].BringToView(tableControl);
                        }).ExecuteLater(5); // Delay to ensure UI is updated
                }
            });
        }
    }
}
