using System;
using System.Collections.Generic;
using System.Linq;

namespace TableForge.UI
{
    internal class CountIfFunction : ExcelFunctionBase
    {
        public override string Name => "COUNTIF";
        protected override ArgumentDefinitionCollection ArgumentDefinitions { get; } = new ArgumentDefinitionCollection(new List<ArgumentDefinition>
        {
            new(ArgumentType.Range),
            new(ArgumentType.Criteria)
        });

        public override object Evaluate(List<object> args, FunctionContext context)
        {
            var range = (List<Cell>) args[0];
            var condition = (Func<object, bool>) args[1];
            
            return range.Count(cell => condition(cell.GetValue()));
        }
    }
}