using System.Collections.Generic;

namespace TableForge.Editor.UI
{
    internal class SumFunction : ExcelFunctionBase
    {
        protected override FunctionInfo FunctionInfo { get; } = new(
            "SUM",
            "Returns the sum of all arguments.",
            FunctionReturnType.Number,
            new ArgumentDefinitionCollection(new List<ArgumentDefinition>
            {
                new(ArgumentType.Number, "value1"),
                new(ArgumentType.Number, "value2", true, true)
            })
        );
        
        public override object Evaluate(List<object> args, FunctionContext context)
        {
            double sum = 0;
            foreach (var arg in args)
            {
                if (arg.TryParseNumber(out var value))
                {
                    sum += value;
                    continue;
                }
                
                if(arg is List<Cell> cells)
                {
                    foreach (var cell in cells)
                    {
                        if (cell.IsNumeric() && cell.GetValue().TryParseNumber(out value))
                            sum += value;
                    }
                }
            }
            return sum;
        }
    }
}