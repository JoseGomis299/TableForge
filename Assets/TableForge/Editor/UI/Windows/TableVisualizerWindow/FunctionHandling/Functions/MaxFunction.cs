using System;
using System.Collections.Generic;

namespace TableForge.Editor.UI
{
    internal class MaxFunction : ExcelFunctionBase
    {
        protected override FunctionInfo FunctionInfo { get; } = new(
            "MAX",
            "Returns the maximum value from a list of numbers or cells containing numbers.",
            FunctionReturnType.Number,
            new ArgumentDefinitionCollection(new List<ArgumentDefinition>
            {
                new(ArgumentType.Number, "value1"),
                new(ArgumentType.Number, "value2",true, true)
            })
        );
        
        public override object Evaluate(List<object> args, FunctionContext context)
        {
            double max = double.MinValue;
            bool foundValue = false;
            
            foreach (var arg in args)
            {
                if (arg.TryParseNumber(out var value))
                {
                    max = Math.Max(max, value);
                    foundValue = true;
                    continue;
                }

                if (arg is List<Cell> cells)
                {
                    foreach (var cell in cells)
                    {
                        if (cell.GetValue().TryParseNumber(out value))
                        {
                            max = Math.Max(max, value);
                            foundValue = true;
                        }
                    }
                }
            }
            
            return foundValue ? max : 0;
        }
    }
}