using System.Collections.Generic;

namespace TableForge.Editor.UI
{
    internal class NotFunction : ExcelFunctionBase
    {
        protected override FunctionInfo FunctionInfo { get; } = new(
            "NOT",
            "Returns the logical negation of a boolean value. If the argument is TRUE, it returns FALSE; if the argument is FALSE, it returns TRUE.",
            FunctionReturnType.Boolean,
            new ArgumentDefinitionCollection(new List<ArgumentDefinition>
            {
                new(ArgumentType.Boolean, "logical_expression")
            })
        );
        
        public override object Evaluate(List<object> args, FunctionContext context)
        {
            return !FunctionArgumentHelper.ConvertToBoolean(args[0]);
        }
    }
}