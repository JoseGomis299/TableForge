using System;
using System.Collections.Generic;

namespace TableForge.Editor.UI
{
    internal class RoundFunction : ExcelFunctionBase
    {
        protected override FunctionInfo FunctionInfo { get; } = new(
            "ROUND",
            "Rounds a number to the specified number of digits.",
            FunctionReturnType.Number,
            new ArgumentDefinitionCollection(new List<ArgumentDefinition>
            {
                new(ArgumentType.SingleNumber, "value"),
                new(ArgumentType.SingleNumber, "decimals",true)  
            })
        );
        
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