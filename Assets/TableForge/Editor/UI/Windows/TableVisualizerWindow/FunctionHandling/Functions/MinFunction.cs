using System;
using System.Collections.Generic;

namespace TableForge.UI
{
    internal class MinFunction : ExcelFunctionBase
    {
        public override string Name => "MIN";
        protected override ArgumentDefinitionCollection ArgumentDefinitions { get; } = new ArgumentDefinitionCollection(new List<ArgumentDefinition>
        {
            new(ArgumentType.Number),
            new(ArgumentType.Number, true, true)
        });
        
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