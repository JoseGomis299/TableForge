using System.Collections.Generic;

namespace TableForge.Editor.UI
{
    internal class IfFunction : ExcelFunctionBase
    {
        protected override FunctionInfo FunctionInfo { get; } = new(
            "IF",
            "Returns one value if a condition is true and another value if it is false.",
            FunctionReturnType.Any,
            new ArgumentDefinitionCollection(new List<ArgumentDefinition>
            {
                new(ArgumentType.Boolean, "logical_expression"),
                new(ArgumentType.Value, "value_if_true"),
                new(ArgumentType.Value, "value_if_false") 
            })
        );
        
        public override object Evaluate(List<object> args, FunctionContext context)
        {
            bool condition = FunctionArgumentHelper.ConvertToBoolean(args[0]);
            return condition ? args[1] : args[2];
        }
    }
}