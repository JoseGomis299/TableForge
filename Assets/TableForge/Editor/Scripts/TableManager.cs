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
        
        [MenuItem("TableForge/Generate Tables")]
        public static List<Table> GenerateTables()
        {
            _tables.Clear();
            
            ItemSelector itemSelector = new ScriptableObjectSelector(new []{"Assets/TableForgeDemoFiles/pruebaTest2.asset", "Assets/TableForgeDemoFiles/Test1.asset", "Assets/TableForgeDemoFiles/Test1 1.asset"});
            List<List<ITFSerializedObject>> items = itemSelector.GetItemData();
            
            foreach (var item in items)
            {
                //TODO: Add modal window to get table name
                string tableName = item[0].Name;
                Table table = TableGenerator.GenerateTable(item, tableName, null);
                _tables.Add(table);
            }
            
            return _tables;
        }
        
    }
}