using System;
using System.Collections.Generic;

namespace TableForge.UI
{
    internal class RoundFunction : ExcelFunctionBase
    {
        public override string Name => "ROUND";
        protected override ArgumentDefinitionCollection ArgumentDefinitions { get; } = new ArgumentDefinitionCollection(new List<ArgumentDefinition>
        {
            new(ArgumentType.Number), // Value to round
            new(ArgumentType.Number, true)  // Decimal places
        });
        
        public override object Evaluate(List<object> args, FunctionContext context)
        {
            double decimals = 0;
            if(args.Count > 1 && !FunctionArgumentHelper.TryGetSingleNumber(args[1], out decimals))
            {
                throw new ArgumentException("ROUND function requires a numeric value for decimal places.");
            }
            
            if (FunctionArgumentHelper.TryGetSingleNumber(args[0], out double value))
            {
                return Math.Round(value, (int)decimals);
            }
         
            throw new AggregateException("ROUND function requires a numeric value to round.");
        }
    }
}