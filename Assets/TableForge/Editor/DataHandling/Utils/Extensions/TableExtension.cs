using TableForge.Editor;

namespace TableForge
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
    }
}