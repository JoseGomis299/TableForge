namespace TableForge.Editor.UI
{
    /// <summary>
    ///  Represents how a table should be displayed and the possible actions that can be performed on it.
    /// </summary>
    public class TableAttributes
    {
        /// <summary>
        /// Specifies whether the sub table supports row addition and deletion or not.
        /// </summary>
        public TableType tableType;

        /// <summary>
        /// Specifies the type of reordering that is allowed in the sub table rows.
        /// </summary>
        public TableReorderMode rowReorderMode;

        /// <summary>
        /// Specifies the type of reordering that is allowed in the sub table columns.
        /// </summary>
        public TableReorderMode columnReorderMode;

        /// <summary>
        /// Specifies the visibility of the headers in the sub table rows.
        /// </summary>
        public TableHeaderVisibility rowHeaderVisibility;

        /// <summary>
        /// Specifies the visibility of the headers in the sub table columns.
        /// </summary>
        public TableHeaderVisibility columnHeaderVisibility;
        
        public TableAttributes(TableType tableType, TableReorderMode rowReorderMode, TableReorderMode columnReorderMode, TableHeaderVisibility rowHeaderVisibility, TableHeaderVisibility columnHeaderVisibility)
        {
            this.tableType = tableType;
            this.rowReorderMode = rowReorderMode;
            this.columnReorderMode = columnReorderMode;
            this.rowHeaderVisibility = rowHeaderVisibility;
            this.columnHeaderVisibility = columnHeaderVisibility;
        }
        
        public TableAttributes() { }
    }
}