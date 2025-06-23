using System.Collections.Generic;
using System.Linq;

namespace TableForge.Editor.UI
{
    internal class CountFunction : ExcelFunctionBase
    {
        protected override FunctionInfo FunctionInfo { get; } = new(
            "COUNT",
            "Counts the number of non-null values in a set.",
            FunctionReturnType.Number,
            new ArgumentDefinitionCollection(new List<ArgumentDefinition>
            {
                new(ArgumentType.Reference, "value1"),
                new(ArgumentType.Reference, "value2",true, true)
            })
        );
        
        public override object Evaluate(List<object> args, FunctionContext context)
        {
            int count = 0;
            foreach (var arg in args)
            {
                var cells = (List<Cell>) arg;
                count += cells.Count(c => c.GetValue() != null);
            }
            return count;
        }
    }
}