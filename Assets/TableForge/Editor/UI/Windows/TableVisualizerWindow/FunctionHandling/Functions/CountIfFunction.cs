using System;
using System.Collections.Generic;
using System.Linq;

namespace TableForge.Editor.UI
{
    internal class CountIfFunction : ExcelFunctionBase
    {
        protected override FunctionInfo FunctionInfo { get; } = new(
            "COUNTIF",
            "Counts the number of cells in a range that meet a specified condition.",
            FunctionReturnType.Number,
            new ArgumentDefinitionCollection(new List<ArgumentDefinition>
            {
                new(ArgumentType.Range, "range"),
                new(ArgumentType.Criteria, "criteria")
            })
        );

        public override object Evaluate(List<object> args, FunctionContext context)
        {
            var range = (List<Cell>) args[0];
            var condition = (Func<object, bool>) args[1];
            
            return range.Count(cell => condition(cell.GetValue()));
        }
    }
}