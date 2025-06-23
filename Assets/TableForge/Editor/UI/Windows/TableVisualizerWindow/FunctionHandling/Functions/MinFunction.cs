using System;
using System.Collections.Generic;

namespace TableForge.Editor.UI
{
    internal class MinFunction : ExcelFunctionBase
    {
        protected override FunctionInfo FunctionInfo { get; } = new(
            "MIN",
            "Returns the minimum value from a list of numbers or cells containing numbers.",
            FunctionReturnType.Number,
            new ArgumentDefinitionCollection(new List<ArgumentDefinition>
            {
                new(ArgumentType.Number, "value1"),
                new(ArgumentType.Number, "value2", true, true)
            })
        );
        
        public override object Evaluate(List<object> args, FunctionContext context)
        {
            double min = double.MaxValue;
            bool foundValue = false;
            
            foreach (var arg in args)
            {
                if (arg.TryParseNumber(out var value))
                {
                    min = Math.Min(min, value);
                    foundValue = true;
                    continue;
                }

                if (arg is List<Cell> cells)
                {
                    foreach (var cell in cells)
                    {
                        if (cell.GetValue().TryParseNumber(out value))
                        {
                            min = Math.Min(min, value);
                            foundValue = true;
                        }
                    }
                }
            }
            
            return foundValue ? min : 0;
        }
    }
}