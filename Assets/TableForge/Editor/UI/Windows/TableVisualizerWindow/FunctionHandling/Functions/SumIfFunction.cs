using System;
using System.Collections.Generic;

namespace TableForge.UI
{
    internal class SumIfFunction : ExcelFunctionBase
    {
        public override string Name => "SUMIF";
        protected override ArgumentDefinitionCollection ArgumentDefinitions { get; } = new ArgumentDefinitionCollection(new List<ArgumentDefinition>
        {
            new(ArgumentType.Range),
            new(ArgumentType.Criteria),
            new(ArgumentType.Range, true) 
        });
        
        public override object Evaluate(List<object> args, FunctionContext context)
        {
            var range = (List<Cell>) args[0];
            var condition = (Func<object, bool>) args[1];
            List<Cell> sumRange = args.Count == 3 ? (List<Cell>) args[2] : range;

            if (sumRange.Count != range.Count)
                throw new ArgumentException("Sum range must have the same number of cells as the condition range.");

            double sum = 0;
            for (int i = 0; i < range.Count; i++)
            {
                if (condition(range[i].GetValue()))
                {
                    if(sumRange[i].IsNumeric() && sumRange[i].GetValue().TryParseNumber(out var value))
                        sum += value;
                }
            }
            return sum;
        }
    }
}