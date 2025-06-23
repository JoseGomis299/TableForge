using System.Collections.Generic;

namespace TableForge.Editor.UI
{
    internal class AndFunction : ExcelFunctionBase
    {
        protected override FunctionInfo FunctionInfo { get; } = new(
            "AND",
            "Returns TRUE if all arguments are TRUE, otherwise returns FALSE.",
            FunctionReturnType.Boolean,
            new ArgumentDefinitionCollection(new List<ArgumentDefinition>
            {
                new(ArgumentType.Boolean, "logical_expression1"),
                new(ArgumentType.Boolean, "logical_expression2",true, true) 
            })
        );
        
        public override object Evaluate(List<object> args, FunctionContext context)
        {
            foreach (var arg in args)
            {
                if (!FunctionArgumentHelper.ConvertToBoolean(arg))
                {
                    return false;
                }
            }
            return true;
        }
    }
}