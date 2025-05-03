using System;
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
            SelectedType = table.IsTypeBound ? Type.GetType(table.BindingTypeName) : null;
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
    
            if (UsePathsMode)
            {
                string[] guids = SelectedAssets.Select(AssetDatabase.GetAssetPath).Select(AssetDatabase.AssetPathToGUID).ToArray();
                _tableMetadata.SetItemGUIDs(guids);
                _tableMetadata.SetBindingType(null);
            }
            else
            {
                _tableMetadata.SetBindingType(SelectedType);
            }
            
            OnTableUpdated?.Invoke(_tableMetadata);
        }
    }
}