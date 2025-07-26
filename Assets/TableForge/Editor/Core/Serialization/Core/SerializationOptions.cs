namespace TableForge.Editor.Serialization
{
    internal class SerializationOptions
    {
        public bool ModifySubTables { get; set; } = true;
        public bool SubTablesAsJson { get; set; } = true;
        public bool CsvCompatible { get; set; } = false;
        public string RowSeparator { get; set; } = SerializationConstants.DefaultRowSeparator;
        public string CancelledRowSeparator { get; set; } = SerializationConstants.DefaultCancelledRowSeparator;
        public string ColumnSeparator { get; set; } = SerializationConstants.DefaultColumnSeparator;
        public string CancelledColumnSeparator { get; set; } = SerializationConstants.DefaultCancelledColumnSeparator;
    }
}