namespace TableForge.Editor.UI
{
    internal class FunctionContext
    {
        public Table BaseTable { get; }

        public FunctionContext(Table baseTable)
        {
            BaseTable = baseTable;
        }
    }
}