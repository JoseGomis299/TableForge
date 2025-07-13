using System.Collections.Generic;

namespace TableForge.Editor
{
    /// <summary>
    /// Entry point for generating TableForge tables.
    /// </summary>
    internal static class TableManager
    {
        public static Table GenerateTable(string[] paths, string tableName)
        {
            ItemSelector itemSelector = new ScriptableObjectSelector(paths);
            List<List<ITfSerializedObject>> items = itemSelector.GetItemData();
            
            if (items.Count == 0)
                return null;
            
            Table table = TableGenerator.GenerateTable(items[0], tableName, null);
            return table;
        }
        
    }
}