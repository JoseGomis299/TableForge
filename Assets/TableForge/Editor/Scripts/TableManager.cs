using System.Collections.Generic;
using UnityEditor;

namespace TableForge
{
    /// <summary>
    /// Entry point for generating TableForge tables.
    /// </summary>
    internal static class TableManager
    {
        private static List<Table> _tables = new List<Table>();
        private static ItemSelector _itemSelector = new ScriptableObjectSelector();
        
        [MenuItem("TableForge/Generate Tables")]
        public static void GenerateTables()
        {
            _tables.Clear();
            List<List<ITFSerializedObject>> items = _itemSelector.GetItemData();
            
            foreach (var item in items)
            {
                //TODO: Add modal window to get table name
                string tableName = item[0].Name;
                Table table = TableGenerator.GenerateTable(item, tableName, null);
                _tables.Add(table);
            }
            
            _itemSelector.Close();
        }
        
    }
}