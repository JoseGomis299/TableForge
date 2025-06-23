using System.Collections.Generic;

namespace TableForge.Editor.UI
{
    internal abstract class ExcelFunctionBase : IExcelFunction
    {
        public string Name => FunctionInfo.Name;
        public string Description => FunctionInfo.Description;
        public FunctionReturnType ReturnType => FunctionInfo.ReturnType;
        public ArgumentDefinitionCollection ExpectedArguments => FunctionInfo.ExpectedArguments;
        protected abstract FunctionInfo FunctionInfo { get; }

        public string GetInfo() => FunctionInfo.ToString();
        public bool ValidateArguments(List<object> args) =>
            FunctionInfo.ExpectedArguments.ValidateArguments(args);
        public abstract object Evaluate(List<object> args, FunctionContext context);
    }
}