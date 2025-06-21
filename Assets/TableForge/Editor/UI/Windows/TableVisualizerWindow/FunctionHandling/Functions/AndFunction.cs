using System.Collections.Generic;

namespace TableForge.UI
{
    internal class AndFunction : ExcelFunctionBase
    {
        public override string Name => "AND";
        protected override ArgumentDefinitionCollection ArgumentDefinitions { get; } = new ArgumentDefinitionCollection(new List<ArgumentDefinition>
        {
            new(ArgumentType.Boolean),
            new(ArgumentType.Boolean, true, true) 
        });
        
        public override object Evaluate(List<object> args, FunctionContext context)
        {
            foreach (var arg in args)
            {
                if (!FunctionArgumentHelper.ConvertToBoolean(arg))
                {
                    return false;
                }
            }
            return true;
        }
    }
}