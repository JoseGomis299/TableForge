using System.Collections.Generic;

namespace TableForge.UI
{
    internal abstract class ExcelFunctionBase : IExcelFunction
    {
        public abstract string Name { get; }
        public IReadOnlyList<ArgumentDefinition> ExpectedArguments => ArgumentDefinitions.Definitions;
        
        protected abstract ArgumentDefinitionCollection ArgumentDefinitions { get; }

        public bool ValidateArguments(List<object> args) =>
            ArgumentDefinitions.ValidateArguments(args);
        public abstract object Evaluate(List<object> args, FunctionContext context);
    }
}