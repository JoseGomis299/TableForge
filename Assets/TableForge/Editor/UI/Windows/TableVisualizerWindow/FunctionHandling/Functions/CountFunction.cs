using System.Collections.Generic;
using System.Linq;

namespace TableForge.UI
{
    internal class CountFunction : ExcelFunctionBase
    {
        public override string Name => "COUNT";
        protected override ArgumentDefinitionCollection ArgumentDefinitions { get; } = new ArgumentDefinitionCollection(new List<ArgumentDefinition>
        {
            new(ArgumentType.Reference),
            new(ArgumentType.Reference, true, true)
        });
        
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