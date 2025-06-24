using System;
using System.Collections.Generic;

namespace TableForge.Editor.UI
{
    internal class AbsFunction : ExcelFunctionBase
    {
        protected override FunctionInfo FunctionInfo { get; } = new(
            "ABS",
            "Returns the absolute value of a number.",
            FunctionReturnType.Number,
            new ArgumentDefinitionCollection(new List<ArgumentDefinition>
            {
                new(ArgumentType.SingleNumber, "value")
            })
        );
        
        public override object Evaluate(List<object> args, FunctionContext context)
        {
            if (FunctionArgumentHelper.TryGetSingleNumber(args[0], out double number))
            {
                return Math.Abs(number);
            }
            
            throw new ArgumentException("ABS function requires a numeric argument.");
        }
    }
}