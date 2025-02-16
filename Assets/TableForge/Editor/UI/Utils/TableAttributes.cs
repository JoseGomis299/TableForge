namespace TableForge.UI
{
    /// <summary>
    ///  Represents how a table should be displayed and the possible actions that can be performed on it.
    /// </summary>
    public struct TableAttributes
    {
        /// <summary>
        /// Specifies whether the sub table supports row addition and deletion or not.
        /// </summary>
        public TableType TableType;

        /// <summary>
        /// Specifies the type of reordering that is allowed in the sub table rows.
        /// </summary>
        public TableReorderMode RowReorderMode;

        /// <summary>
        /// Specifies the type of reordering that is allowed in the sub table columns.
        /// </summary>
        public TableReorderMode ColumnReorderMode;

        /// <summary>
        /// Specifies the visibility of the headers in the sub table rows.
        /// </summary>
        public TableHeaderVisibility RowHeaderVisibility;

        /// <summary>
        /// Specifies the visibility of the headers in the sub table columns.
        /// </summary>
        public TableHeaderVisibility ColumnHeaderVisibility;
        
        public TableAttributes(TableType tableType, TableReorderMode rowReorderMode, TableReorderMode columnReorderMode, TableHeaderVisibility rowHeaderVisibility, TableHeaderVisibility columnHeaderVisibility)
        {
            TableType = tableType;
            RowReorderMode = rowReorderMode;
            ColumnReorderMode = columnReorderMode;
            RowHeaderVisibility = rowHeaderVisibility;
            ColumnHeaderVisibility = columnHeaderVisibility;
        }
    }
}