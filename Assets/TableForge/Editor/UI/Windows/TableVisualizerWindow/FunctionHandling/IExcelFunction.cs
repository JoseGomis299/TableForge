using System.Collections.Generic;

namespace TableForge.Editor.UI
{
    internal interface IExcelFunction
    {
        ArgumentDefinitionCollection ExpectedArguments { get; }
        string Name { get; }
        string Description { get; }
        FunctionReturnType ReturnType { get; }
        string GetInfo();
        bool ValidateArguments(List<object> args);
        object Evaluate(List<object> args, FunctionContext context);
    }
}