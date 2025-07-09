using System.Collections;
using System.Collections.Generic;

namespace TableForge.Editor
{
    internal static class TableExtension
    {
        public static Cell GetFirstCell(this Table table)
        {
            if (table == null || table.Rows.Count == 0)
                return null;
            
            return table.GetCell(1, 1);
        }
        
        public static Cell GetLastCell(this Table table)
        {
            if (table == null || table.Rows.Count == 0)
                return null;
            
            return table.GetCell(table.Columns.Count, table.Rows.Count);
        }
        
        public static List<string> GetFlatteredColumnNames(this Table table)
        {
            List<string> columnNames = new List<string>();
            if (table == null || table.Columns.Count == 0 || table.Rows.Count == 0)
                return columnNames;
            
            var fields = table.Rows[1].SerializedObject.SerializedType.Fields;
            foreach (var field in fields)
            {
                if (!SerializationUtil.IsTableForgeSerializable(TypeMatchMode.Exact, field.Type, out _) && !field.Type.IsSimpleType()) 
                    columnNames.AddRange(GetSubTableColumnNames(field));
                else 
                    columnNames.Add(field.FriendlyName);
            }
            
            return columnNames;
        }
        
        private static List<string> GetSubTableColumnNames(TfFieldInfo field)
        {
            if (field.Type.ImplementsInterface(typeof(ICollection)) || field.Type.ImplementsInterface(typeof(ICollection<>)))
                return new List<string> { field.FriendlyName };

            List<string> columnNames = new List<string>();
            var fields = SerializationUtil.GetSerializableFields(field.Type, field.FieldInfo);
            foreach (var subField in fields)
            {
                if (!SerializationUtil.IsTableForgeSerializable(TypeMatchMode.Exact, subField.Type, out _) && !subField.Type.IsSimpleType()) 
                    columnNames.AddRange(GetSubTableColumnNames(subField));
                else 
                    columnNames.Add(subField.FriendlyName);
            }
            
            return columnNames;
        }
        
    }
}