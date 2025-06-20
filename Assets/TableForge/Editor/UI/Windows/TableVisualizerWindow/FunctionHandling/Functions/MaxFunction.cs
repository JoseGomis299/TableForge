using System;
using System.Collections.Generic;

namespace TableForge.UI
{
    internal class MaxFunction : ExcelFunctionBase
    {
        public override string Name => "MAX";
        protected override ArgumentDefinitionCollection ArgumentDefinitions { get; } = new ArgumentDefinitionCollection(new List<ArgumentDefinition>
        {
            new(ArgumentType.Number),
            new(ArgumentType.Number, true, true)
        });
        
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