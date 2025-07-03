namespace TableForge.Editor
{
    /// <summary>
    /// Concrete implementation of <see cref="CellAnchor"/> representing a column in a table.
    /// </summary>
    internal class Column : CellAnchor
    {
        public Column(string name, int position, Table table) : base(name, position, table)
        {
            Id = HashCodeUtil.CombineHashes(name, table.Name);
            table.AddColumn(this);
        }
    }
}