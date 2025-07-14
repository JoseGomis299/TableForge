namespace TableForge.Editor.UI
{
    public enum TableType
    {
        /// <summary>
        /// Table with a fixed structure, where columns and rows are predefined.
        /// </summary>
        Static,
        /// <summary>
        /// Table with a dynamic structure, where columns can be added or removed at runtime.
        /// </summary>
        Dynamic,
        /// <summary>
        /// Static table that is converted to a dynamic table when it becomes empty. (e.g. a sub-table with a null value)
        /// </summary>
        DynamicIfEmpty
    }
}