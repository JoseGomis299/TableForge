using System.Collections.Generic;

namespace TableForge.Editor.UI
{
    internal class XorFunction : ExcelFunctionBase
    {
        protected override FunctionInfo FunctionInfo { get; } = new(
            "XOR",
            "Returns TRUE if exactly one argument is TRUE, otherwise returns FALSE.",
            FunctionReturnType.Boolean,
            new ArgumentDefinitionCollection(new List<ArgumentDefinition>
            {
                new(ArgumentType.Boolean, "logical_expression1"),
                new(ArgumentType.Boolean, "logical_expression2", true, true) 
            })
        );
        
        public override object Evaluate(List<object> args, FunctionContext context)
        {
            int trueCount = 0;
            foreach (var arg in args)
            {
                if (FunctionArgumentHelper.ConvertToBoolean(arg))
                {
                    trueCount++;
                    if (trueCount > 1) // More than one true means XOR is false
                        return false;
                }
            }
            return trueCount == 1; // XOR is true only if exactly one argument is true
        }
    }
}