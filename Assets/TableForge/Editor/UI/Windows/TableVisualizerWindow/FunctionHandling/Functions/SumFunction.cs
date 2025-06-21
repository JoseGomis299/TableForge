using System.Collections.Generic;

namespace TableForge.UI
{
    internal class SumFunction : ExcelFunctionBase
    {
        public override string Name => "SUM";
        protected override ArgumentDefinitionCollection ArgumentDefinitions { get; } = new ArgumentDefinitionCollection(new List<ArgumentDefinition>
        {
            new(ArgumentType.Number),
            new(ArgumentType.Number, true, true)
        });
        
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