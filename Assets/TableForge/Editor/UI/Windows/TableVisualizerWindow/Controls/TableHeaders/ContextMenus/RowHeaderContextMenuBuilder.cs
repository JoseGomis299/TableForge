using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    /// <summary>
    /// Context menu builder for row headers that handles row-specific menu items.
    /// </summary>
    internal class RowHeaderContextMenuBuilder : BaseHeaderContextMenuBuilder
    {
        public override void BuildContextMenu(HeaderControl header, ContextualMenuPopulateEvent evt)
        {
            if (header is not RowHeaderControl rowHeader) return;
            
            // Check if we're currently editing the name
            if (rowHeader.IsChangingName) return;
            
            AddAssetManagementItems(rowHeader, evt);
            evt.menu.AppendSeparator();
            AddExpandCollapseItems(header, evt);
            evt.menu.AppendSeparator();
            AddSortingItems(header, evt);
            evt.menu.AppendSeparator();
            AddRowOperationItems(rowHeader, evt);
        }
        
        private void AddAssetManagementItems(RowHeaderControl rowHeader, ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Focus asset in Inspector", (_) =>
            {
                var targetObject = ((Row)rowHeader.CellAnchor).SerializedObject.RootObject;
                Selection.activeObject = targetObject;
                EditorGUIUtility.PingObject(targetObject);
            });
            
            evt.menu.AppendAction("Rename asset", (_) =>
            {
                rowHeader.StartNameEditing();
            });
        }
        
        private void AddRowOperationItems(RowHeaderControl rowHeader, ContextualMenuPopulateEvent evt)
        {
            if(!rowHeader.TableControl.Metadata.IsTypeBound)
                evt.menu.AppendAction("Remove this item", (_) => rowHeader.RemoveThisRow());
                
            evt.menu.AppendAction("Delete this asset", (_) =>
            {
                AssetUtils.DeleteAsset(((Row)rowHeader.CellAnchor).SerializedObject.RootObjectGuid, rowHeader.RemoveThisRow);
            });
            
            evt.menu.AppendSeparator();

            if (rowHeader.TableControl.CellSelector.GetSelectedRows(rowHeader.TableControl.TableData).Count > 1)
            {
                if (!rowHeader.TableControl.Metadata.IsTypeBound)
                    evt.menu.AppendAction("Remove selected items", (_) => rowHeader.RemoveSelectedRows());
                    
                evt.menu.AppendAction("Delete associated assets", (_) =>
                {
                    AssetUtils.DeleteAssets(
                        rowHeader.TableControl.CellSelector.GetSelectedRows(rowHeader.TableControl.TableData)
                            .Select(x => x.SerializedObject.RootObjectGuid), 
                        rowHeader.RemoveSelectedRows);
                });
            }
        }
    }
} 