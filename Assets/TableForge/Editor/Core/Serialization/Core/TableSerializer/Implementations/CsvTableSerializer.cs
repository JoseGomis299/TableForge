using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace TableForge.Editor.Serialization
{
    internal class CsvTableSerializer : TableSerializer
    {
        public bool FlattenSubTables { get; } 
        
        public CsvTableSerializer(Table table, bool includeRowGuids, bool includeRowPaths, bool flattenSubTables) 
            : base(table, SerializationFormat.Csv, includeRowGuids, includeRowPaths)
        {
            FlattenSubTables = flattenSubTables;
        }

        public override string Serialize(int maxRowCount = -1)
        {
            //Change the serialization settings
            bool serializeSubTablesAsJson = SerializationConstants.subTablesAsJson;
            SerializationConstants.csvCompatible = true; // We always quote necessary values in CSV serialization.
            SerializationConstants.subTablesAsJson = !FlattenSubTables; // If we are flattening sub-tables, we do not serialize them as JSON.
            SerializationConstants.columnSeparator = SerializationConstants.CsvColumnSeparator;
            SerializationConstants.rowSeparator = SerializationConstants.CsvRowSeparator;
            SerializationConstants.cancelledColumnSeparator = SerializationConstants.CsvCancelledColumnSeparator;
            SerializationConstants.cancelledRowSeparator = SerializationConstants.CsvCancelledRowSeparator;
            
            StringBuilder serializedData = new StringBuilder();
            if(Table == null || Table.Rows.Count == 0 || Table.Columns.Count == 0)
            {
                return string.Empty; 
            }
            
            //Write optional row guids and paths
            if(IncludeRowGuids)
                serializedData.Append("Guid").Append(SerializationConstants.CsvColumnSeparator);
            
            if(IncludeRowPaths)
                serializedData.Append("Path").Append(SerializationConstants.CsvColumnSeparator);
            
            //Retrieve and write the column names
            List<string> columnNames = FlattenSubTables ? Table.GetFlatteredColumnNames() : Table.OrderedColumns.Select(c => c.Name).ToList();
            foreach (var column in columnNames)
            {
                serializedData.Append(column).Append(SerializationConstants.CsvColumnSeparator);
            }
            
            // Remove the last column separator and add a newline
            serializedData.Remove(serializedData.Length - SerializationConstants.CsvColumnSeparator.Length, SerializationConstants.CsvColumnSeparator.Length);
            serializedData.Append(SerializationConstants.rowSeparator);
            
            // Write the data rows
            int rowCount = maxRowCount > 0 ? maxRowCount : Table.Rows.Count;
            foreach (var row in Table.OrderedRows)
            {
                if (rowCount <= 0) break; // Stop if we reached the max row count
                if (IncludeRowGuids)
                    serializedData.Append(row.SerializedObject.RootObjectGuid).Append(SerializationConstants.CsvColumnSeparator);
                
                if (IncludeRowPaths)
                    serializedData.Append(AssetDatabase.GetAssetPath(row.SerializedObject.RootObject)).Append(SerializationConstants.CsvColumnSeparator);
                
                foreach (var cell in row.OrderedCells)
                {
                    serializedData.Append(cell.SerializeCellCsvCompatible(FlattenSubTables)).Append(SerializationConstants.CsvColumnSeparator);
                }
                
                // Remove the last column separator and add a newline
                serializedData.Remove(serializedData.Length - SerializationConstants.CsvColumnSeparator.Length, SerializationConstants.CsvColumnSeparator.Length);
                serializedData.Append(SerializationConstants.rowSeparator);
                rowCount--;
            }
            
            // Remove the last row separator if it exists
            if (serializedData.Length > 0 && serializedData[^1] == SerializationConstants.rowSeparator[0])
            {
                serializedData.Remove(serializedData.Length - SerializationConstants.rowSeparator.Length, SerializationConstants.rowSeparator.Length);
            }
            
            // Restore the original setting
            SerializationConstants.subTablesAsJson = serializeSubTablesAsJson;
            SerializationConstants.csvCompatible = false;
            SerializationConstants.columnSeparator = SerializationConstants.DefaultColumnSeparator;
            SerializationConstants.rowSeparator = SerializationConstants.DefaultRowSeparator;
            SerializationConstants.cancelledColumnSeparator = SerializationConstants.DefaultCancelledColumnSeparator;
            SerializationConstants.cancelledRowSeparator = SerializationConstants.DefaultCancelledRowSeparator;
            
            return serializedData.ToString();
        }
    }
}