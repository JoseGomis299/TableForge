using System;
using System.Collections.Generic;

namespace TableForge.Editor.UI
{
    internal class ModFunction : ExcelFunctionBase
    {
        protected override FunctionInfo FunctionInfo { get; } = new(
            "MOD",
            "Returns the remainder after a number is divided by a divisor.",
            FunctionReturnType.Number,
            new ArgumentDefinitionCollection(new List<ArgumentDefinition>
            {
                new(ArgumentType.SingleNumber, "dividend"),
                new(ArgumentType.SingleNumber, "divisor") 
            })
        );
        
        public override object Evaluate(List<object> args, FunctionContext context)
        {
            if (FunctionArgumentHelper.TryGetSingleNumber(args[0], out double dividend) && 
                FunctionArgumentHelper.TryGetSingleNumber(args[1], out double divisor))
            {
                if (Math.Abs(divisor) < double.Epsilon)
                {
                    throw new ArgumentException("Division by zero in MOD function.");
                }
                return dividend % divisor;
            }
         
            throw new ArgumentException("MOD function requires numeric arguments.");
        }
    }
}