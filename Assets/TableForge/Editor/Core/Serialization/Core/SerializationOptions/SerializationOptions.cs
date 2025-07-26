namespace TableForge.Editor.Serialization
{
    internal class SerializationOptions
    {
        public bool ModifySubTables { get; protected set; } = true;
        public bool SubTablesAsJson { get; set; } = true;
        public bool CsvCompatible { get; protected set; } = false;
        public string RowSeparator { get; protected set; } = SerializationConstants.DefaultRowSeparator;
        public string CancelledRowSeparator { get; protected set; } = SerializationConstants.DefaultCancelledRowSeparator;
        public string ColumnSeparator { get; protected set; } = SerializationConstants.DefaultColumnSeparator;
        public string CancelledColumnSeparator { get; protected set; } = SerializationConstants.DefaultCancelledColumnSeparator;
    }
}