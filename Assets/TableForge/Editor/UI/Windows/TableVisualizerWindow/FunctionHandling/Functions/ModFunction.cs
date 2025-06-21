using System;
using System.Collections.Generic;

namespace TableForge.UI
{
    internal class ModFunction : ExcelFunctionBase
    {
        public override string Name => "MOD";
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
                    throw new ArgumentException("Division by zero in MOD function.");
                }
                return dividend % divisor;
            }
         
            throw new ArgumentException("MOD function requires numeric arguments.");
        }
    }
}