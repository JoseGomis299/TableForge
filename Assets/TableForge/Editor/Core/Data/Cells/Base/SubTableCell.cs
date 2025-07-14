namespace TableForge.Editor
{
    /// <summary>
    /// Represents a cell that contains a subtable, typically used for complex or collection-based fields.
    /// </summary>
    internal abstract class SubTableCell : Cell
    {
        #region Properties

        /// <summary>
        /// The subtable associated with this cell.
        /// </summary>
        public Table SubTable { get; protected set; }

        #endregion

        #region Constructors

        protected SubTableCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo)
        {
        }

        #endregion
        
        #region Public Methods

        public override void RefreshData()
        {
            object value = cachedValue;
            base.RefreshData();
            
            if (value != cachedValue)
                CreateSubTable();
        }
        
        #endregion
        
        #region Protected Methods

        /// <summary>
        /// Creates and initializes the subtable associated with this cell.
        /// </summary>
        protected abstract void CreateSubTable();
        
        #endregion
    }
}