using System.Collections.Generic;
using System.Linq;
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
    }
} 