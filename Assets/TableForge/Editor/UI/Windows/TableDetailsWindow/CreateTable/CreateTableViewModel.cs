using System;
using System.Linq;
using UnityEditor;

namespace TableForge.Editor.UI
{
    internal class CreateTableViewModel : TableDetailsViewModel
    {
        public event Action<TableMetadata> OnTableCreated;

        public void CreateTable()
        {
            if (UsePathsMode)
            {
                string[] guids = selectedAssets.Select(AssetDatabase.GetAssetPath).Select(AssetDatabase.AssetPathToGUID).ToArray();
                OnTableCreated?.Invoke(TableMetadataManager.CreateMetadata(guids, TableName));
            }
            else OnTableCreated?.Invoke(TableMetadataManager.CreateMetadata(SelectedType, TableName));
        }
        
        public override string GetErrors()
        { 
            string error = base.GetErrors();
            if(HasErrors) return error;
            HasErrors = true;
            
            if (TableMetadataManager.LoadMetadata(TableName) != null)
            {
                return $"Table name '{TableName}' already exists.";
            }
            
            HasErrors = false;
            return string.Empty;
        }

        public bool IsDefaultName(string name)
        {
            string[] parts = name.Split(' ');
            return parts.Length switch
            {
                1 => TypeRegistry.TypeNames.Contains(parts[0]),
                2 when int.TryParse(parts[1].TrimEnd(')').TrimStart('('), out int count) => count > 0 && TypeRegistry.TypeNames.Contains(parts[0]),
                _ => false
            };
        }
        
        public string GetDefaultName()
        {
            if(SelectedType == null)
            {
                return "";
            }
            
            string typeName = SelectedType.Name;
            string defaultName = typeName;
            
            int count = 1;
            while (TableMetadataManager.LoadMetadata(defaultName) != null)
            {
                defaultName = $"{typeName} ({count++})";
            }
            return defaultName;
        }
    }
}