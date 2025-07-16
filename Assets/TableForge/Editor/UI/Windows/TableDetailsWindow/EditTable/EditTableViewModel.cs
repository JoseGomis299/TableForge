using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TableForge.Editor.UI
{
    internal class EditTableViewModel : TableDetailsViewModel
    {
        public event Action<TableMetadata> OnTableUpdated;
        
        private readonly TableMetadata _tableMetadata;
        
        public EditTableViewModel(TableMetadata table) 
        {
            TableName = table.Name;
            UsePathsMode = !table.IsTypeBound;
            SelectedType = table.GetItemsType();
            selectedNamespace = string.IsNullOrEmpty(SelectedType?.Namespace) ? "Global" : SelectedType?.Namespace;
            
            if (UsePathsMode)
            {
                var guids = table.ItemGUIDs;
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                    if (asset != null) selectedAssets.Add(asset);
                }
            }
            
            _tableMetadata = table;
        }

        public void UpdateTable()
        {
            TableMetadata oldTableMetadata = TableMetadata.Clone(_tableMetadata);
            _tableMetadata.Name = TableName;
            HashSet<string> removedGuids = new HashSet<string>(_tableMetadata.ItemGUIDs);
    
            if (UsePathsMode)
            {
                string[] guids = selectedAssets.Select(AssetDatabase.GetAssetPath).Select(AssetDatabase.AssetPathToGUID).ToArray();
                _tableMetadata.SetItemsType(AssetDatabase.GetMainAssetTypeFromGUID(new GUID(guids[0])));
                _tableMetadata.SetItemGUIDs(guids);
                _tableMetadata.SetBindingType(null);
            }
            else
            {
                _tableMetadata.SetBindingType(SelectedType);
                _tableMetadata.SetItemsType(SelectedType);
            }

            removedGuids.ExceptWith(_tableMetadata.ItemGUIDs);
            foreach (var guid in removedGuids)
            {
                int anchorId = HashCodeUtil.CombineHashes(guid);
                _tableMetadata.RemoveAnchorMetadata(anchorId);
            }
            
            EditTableCommand command = new EditTableCommand(
                oldTableMetadata,
                TableMetadata.Clone(_tableMetadata), 
                UpdateTable
            );
            UndoRedoManager.AddToQueue(command);
            OnTableUpdated?.Invoke(_tableMetadata);
        }

        private void UpdateTable(TableMetadata tableMetadata)
        {
            TableMetadata.Copy(_tableMetadata, tableMetadata);
            OnTableUpdated?.Invoke(_tableMetadata);
        }
    }
}