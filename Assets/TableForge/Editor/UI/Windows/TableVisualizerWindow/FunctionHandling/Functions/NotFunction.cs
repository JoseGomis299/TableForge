using System.Collections.Generic;

namespace TableForge.UI
{
    internal class NotFunction : ExcelFunctionBase
    {
        public override string Name => "NOT";
        protected override ArgumentDefinitionCollection ArgumentDefinitions { get; } = new ArgumentDefinitionCollection(new List<ArgumentDefinition>
        {
            new(ArgumentType.Boolean)
        });
        
        public override object Evaluate(List<object> args, FunctionContext context)
        {
            return !FunctionArgumentHelper.ConvertToBoolean(args[0]);
        }
    }
}