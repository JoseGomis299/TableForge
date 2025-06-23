using System.Collections.Generic;

namespace TableForge.Editor.UI
{
    internal class AverageFunction : ExcelFunctionBase
    {
        protected override FunctionInfo FunctionInfo { get; } = new(
            "AVERAGE",
            "Calculates the average of a set of numbers.",
            FunctionReturnType.Number,
            new ArgumentDefinitionCollection(new List<ArgumentDefinition>
            {
                new(ArgumentType.Number, "value1"),
                new(ArgumentType.Number, "value2",true, true)
            })
        );

        public override object Evaluate(List<object> args, FunctionContext context)
        {
            double sum = 0;
            int count = 0;
            
            foreach (var arg in args)
            {
                if (arg is List<Cell> cells)
                {
                    foreach (var cell in cells)
                    {
                        if (cell.IsNumeric() && cell.GetValue().TryParseNumber(out var value))
                        {
                            sum += value;
                            count++;
                        }
                    }
                }
                else if (arg.TryParseNumber(out var value))
                {
                    sum += value;
                    count++;
                }
            }
            
            return count > 0 ? sum / count : 0;
        }
    }
}