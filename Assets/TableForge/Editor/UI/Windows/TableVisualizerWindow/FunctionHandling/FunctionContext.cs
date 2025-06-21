namespace TableForge.UI
{
    internal class FunctionContext
    {
        public Cell ContextCell { get; }
        public ReferenceParser ReferenceParser { get; }
        public FunctionExecutor FunctionExecutor { get; }

        public FunctionContext(Cell contextCell, ReferenceParser referenceParser, FunctionExecutor executor)
        {
            ContextCell = contextCell;
            ReferenceParser = referenceParser;
            FunctionExecutor = executor;
        }
    }
}