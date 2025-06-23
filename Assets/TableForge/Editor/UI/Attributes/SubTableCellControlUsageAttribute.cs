using System;

namespace TableForge.Editor.UI
{
    /// <summary>
    /// An attribute used to specify how a <see cref="SubTableCellControl"/> should display its sub table and the possible actions that can be performed on it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    internal class SubTableCellControlUsageAttribute : Attribute
    {
        #region Properties

        /// <summary>
        /// The attributes that define how the sub table should be displayed.
        /// </summary>
        public TableAttributes TableAttributes { get; }

        #endregion

        #region Constructors

        /// <summary>
        ///  Initializes a new instance of the <see cref="SubTableCellControlUsageAttribute"/> class.
        /// </summary>
        /// <param name="tableType">Specifies whether the sub table supports row addition and deletion or not.</param>
        /// <param name="reorderMode">Specifies the type of reordering that is allowed in the sub table.</param>
        /// <param name="headerVisibility">Specifies the visibility of the headers in the sub table.</param>
        public SubTableCellControlUsageAttribute(TableType tableType, TableReorderMode reorderMode, TableHeaderVisibility headerVisibility)
        {
            TableAttributes = new TableAttributes(tableType, reorderMode, reorderMode, headerVisibility, headerVisibility);
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SubTableCellControlUsageAttribute"/> class.
        /// </summary>
        /// <param name="tableType">Specifies whether the sub table supports row addition and deletion or not.</param>
        /// <param name="rowReorderMode">Specifies the type of reordering that is allowed in the sub table rows.</param>
        /// <param name="columnReorderMode">Specifies the type of reordering that is allowed in the sub table columns.</param>
        /// <param name="rowHeaderVisibility">Specifies the visibility of the headers in the sub table rows.</param>
        /// <param name="columnHeaderVisibility">Specifies the visibility of the headers in the sub table columns.</param>
        public SubTableCellControlUsageAttribute(TableType tableType, TableReorderMode rowReorderMode, TableReorderMode columnReorderMode, TableHeaderVisibility rowHeaderVisibility, TableHeaderVisibility columnHeaderVisibility)
        {
            TableAttributes = new TableAttributes(tableType, rowReorderMode, columnReorderMode, rowHeaderVisibility, columnHeaderVisibility);
        }
        
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SubTableCellControlUsageAttribute"/> class.
        /// </summary>
        /// <param name="tableType">Specifies whether the sub table supports row addition and deletion or not.</param>
        /// <param name="reorderMode">Specifies the type of reordering that is allowed in the sub table.</param>
        /// <param name="rowHeaderVisibility">Specifies the visibility of the headers in the sub table rows.</param>
        /// <param name="columnHeaderVisibility">Specifies the visibility of the headers in the sub table columns.</param>
        public SubTableCellControlUsageAttribute(TableType tableType, TableReorderMode reorderMode, TableHeaderVisibility rowHeaderVisibility, TableHeaderVisibility columnHeaderVisibility)
        {
            TableAttributes = new TableAttributes(tableType, reorderMode, reorderMode, rowHeaderVisibility, columnHeaderVisibility);
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SubTableCellControlUsageAttribute"/> class.
        /// </summary>
        /// <param name="tableType">Specifies whether the sub table supports row addition and deletion or not.</param>
        /// <param name="rowReorderMode">Specifies the type of reordering that is allowed in the sub table rows.</param>
        /// <param name="columnReorderMode">Specifies the type of reordering that is allowed in the sub table columns.</param>
        /// <param name="headerVisibility">Specifies the visibility of the headers in the sub table.</param>
        public SubTableCellControlUsageAttribute(TableType tableType, TableReorderMode rowReorderMode, TableReorderMode columnReorderMode, TableHeaderVisibility headerVisibility)
        {
            TableAttributes = new TableAttributes(tableType, rowReorderMode, columnReorderMode, headerVisibility, headerVisibility);
        }

        #endregion
    }
}