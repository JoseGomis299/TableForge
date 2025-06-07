using System.Collections.Generic;
using System.Diagnostics;

namespace TableForge
{
    /// <summary>
    /// Entry point for generating TableForge tables.
    /// </summary>
    internal static class TableManager
    {
        public static List<Table> GenerateTables()
        {
            List<Table> tables = new List<Table>();
            ItemSelector itemSelector = new ScriptableObjectSelector(new[] { "Assets/TableForgeDemoFiles" });
            List<List<ITFSerializedObject>> items = itemSelector.GetItemData();
            
            foreach (var item in items)
            {
                string tableName = item[0].Name;
                Table table = TableGenerator.GenerateTable(item, tableName, null);
                tables.Add(table);
            }
            
            return tables;
        }

        public static Table GenerateTable(string[] paths, string tableName)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            ItemSelector itemSelector = new ScriptableObjectSelector(paths);
            List<List<ITFSerializedObject>> items = itemSelector.GetItemData();
            
            stopwatch.Stop();
            UnityEngine.Debug.Log($"Took {stopwatch.ElapsedMilliseconds} ms to load {items.Count} items from {paths.Length} paths.");
            stopwatch.Reset();
            stopwatch.Start();
            
            if (items.Count == 0)
                return null;
            
            Table table = TableGenerator.GenerateTable(items[0], tableName, null);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"Took {stopwatch.ElapsedMilliseconds} ms to generate table '{tableName}' with {table.Rows.Count} rows and {table.Columns.Count} columns.");
            return table;
        }
        
    }
}