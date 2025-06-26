namespace TableForge.Editor.UI
{
    internal class FunctionContext
    {
        public Table BaseTable { get; }
        public ReferenceParser ReferenceParser { get; }
        public FunctionExecutor FunctionExecutor { get; }

        public FunctionContext(Table baseTable, ReferenceParser referenceParser, FunctionExecutor executor)
        {
            ReferenceParser = referenceParser;
            FunctionExecutor = executor;
            BaseTable = baseTable;
        }
    }
}