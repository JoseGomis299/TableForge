using System;
using System.Collections.Generic;

namespace TableForge.UI
{
    internal class DivideFunction : ExcelFunctionBase
    {
        public override string Name => "DIVIDE";
        protected override ArgumentDefinitionCollection ArgumentDefinitions { get; } = new ArgumentDefinitionCollection(new List<ArgumentDefinition>
        {
            new(ArgumentType.Number), // Dividend
            new(ArgumentType.Number)  // Divisor
        });
        
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