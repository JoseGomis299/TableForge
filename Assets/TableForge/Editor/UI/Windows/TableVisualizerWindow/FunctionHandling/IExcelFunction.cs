using System.Collections.Generic;

namespace TableForge.UI
{
    internal interface IExcelFunction
    {
        IReadOnlyList<ArgumentDefinition> ExpectedArguments { get; }
        string Name { get; }
        bool ValidateArguments(List<object> args);
        object Evaluate(List<object> args, FunctionContext context);
    }
}