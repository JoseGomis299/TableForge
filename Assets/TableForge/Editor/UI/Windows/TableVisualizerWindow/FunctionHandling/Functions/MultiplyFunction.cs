using System;
using System.Collections.Generic;

namespace TableForge.Editor.UI
{
    internal class MultiplyFunction : ExcelFunctionBase
    {
        protected override FunctionInfo FunctionInfo { get; } = new(
            "MULTIPLY",
            "Multiplies two numbers together.",
            FunctionReturnType.Number,
            new ArgumentDefinitionCollection(new List<ArgumentDefinition>
            {
                new(ArgumentType.SingleNumber, "value1"),
                new(ArgumentType.SingleNumber, "value2")
            })
        );
        
        public override object Evaluate(List<object> args, FunctionContext context)
        {
            if (FunctionArgumentHelper.TryGetSingleNumber(args[0], out double a) && 
                FunctionArgumentHelper.TryGetSingleNumber(args[1], out double b))
            {
                return a * b;
            }
            
            throw new ArgumentException("MULTIPLY function requires numeric arguments.");
        }
    }
}