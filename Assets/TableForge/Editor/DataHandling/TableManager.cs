using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace TableForge.Editor
{
    /// <summary>
    /// Entry point for generating TableForge tables.
    /// </summary>
    internal static class TableManager
    {
        public static Table GenerateTable(string[] paths, string tableName)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            ItemSelector itemSelector = new ScriptableObjectSelector(paths);
            List<List<ITfSerializedObject>> items = itemSelector.GetItemData();
            
            stopwatch.Stop();
            UnityEngine.Debug.Log($"Took {stopwatch.ElapsedMilliseconds} ms to load {items.Count} items from {paths.Length} paths.");
            stopwatch.Reset();
            stopwatch.Start();
            
            if (items.Count == 0)
                return null;
            
            Table table = TableGenerator.GenerateTable(items[0], tableName, null);
            string serializedTable = TableSerializer.SerializeTable(new CsvTableSerializationArgs(table, true, true, true));
            Debug.Log(serializedTable); 
            serializedTable = TableSerializer.SerializeTable(new CsvTableSerializationArgs(table, true, true, false));
            Debug.Log(serializedTable);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"Took {stopwatch.ElapsedMilliseconds} ms to generate table '{tableName}' with {table.Rows.Count} rows and {table.Columns.Count} columns.");
            return table;
        }
        
    }
}