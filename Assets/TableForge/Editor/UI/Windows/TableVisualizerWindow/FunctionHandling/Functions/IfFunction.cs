using System.Collections.Generic;

namespace TableForge.UI
{
    internal class IfFunction : ExcelFunctionBase
    {
        public override string Name => "IF";
        protected override ArgumentDefinitionCollection ArgumentDefinitions { get; } = new ArgumentDefinitionCollection(new List<ArgumentDefinition>
        {
            new(ArgumentType.LogicExpression),
            new(ArgumentType.Value),
            new(ArgumentType.Value, true) 
        });
        
        public override object Evaluate(List<object> args, FunctionContext context)
        {
            bool condition = (bool) args[0];
            return condition ? args[1] : (args.Count == 3 ? args[2] : null);
        }
    }
}