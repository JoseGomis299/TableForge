using System;
using System.Collections.Generic;

namespace TableForge.UI
{
    internal class DivideFunction : ExcelFunctionBase
    {
        protected override FunctionInfo FunctionInfo { get; } = new FunctionInfo(
            "DIVIDE",
            "Divides the first number by the second number. Returns an error if the divisor is zero.",
            FunctionReturnType.Number,
            new ArgumentDefinitionCollection(new List<ArgumentDefinition>
            {
                new(ArgumentType.Number, "dividend"), 
                new(ArgumentType.Number, "divisor") 
            })
        );
        
        public override object Evaluate(List<object> args, FunctionContext context)
        {
            if (FunctionArgumentHelper.TryGetSingleNumber(args[0], out double dividend) && 
                FunctionArgumentHelper.TryGetSingleNumber(args[1], out double divisor))
            {
                if (Math.Abs(divisor) < double.Epsilon)
                {
                    throw new ArgumentException("Division by zero in DIVIDE function.");

                }
                return dividend / divisor;
            }
         
            throw new ArgumentException("DIVIDE function requires numeric arguments.");
        }
    }
}