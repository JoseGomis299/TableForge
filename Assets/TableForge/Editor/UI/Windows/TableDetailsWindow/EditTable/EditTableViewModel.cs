using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TableForge.UI
{
    internal class EditTableViewModel : TableDetailsViewModel
    {
        public event Action<TableMetadata> OnTableUpdated;
        
        private TableMetadata _tableMetadata;
        
        public EditTableViewModel(TableMetadata table) 
        {
            TableName = table.Name;
            UsePathsMode = !table.IsTypeBound;
            SelectedType = table.GetItemsType();
            SelectedNamespace = string.IsNullOrEmpty(SelectedType?.Namespace) ? "Global" : SelectedType?.Namespace;
            
            if (UsePathsMode)
            {
                var guids = table.ItemGUIDs;
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                    if (asset != null) SelectedAssets.Add(asset);
                }
            }
            
            _tableMetadata = table;
        }

        public void UpdateTable()
        {
            _tableMetadata.Name = TableName;
            HashSet<string> removedGuids = new HashSet<string>(_tableMetadata.ItemGUIDs);
    
            if (UsePathsMode)
            {
                string[] guids = SelectedAssets.Select(AssetDatabase.GetAssetPath).Select(AssetDatabase.AssetPathToGUID).ToArray();
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
            
            OnTableUpdated?.Invoke(_tableMetadata);
        }
    }
}