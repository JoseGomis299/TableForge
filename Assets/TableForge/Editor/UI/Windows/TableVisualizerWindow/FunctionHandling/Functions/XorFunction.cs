using System.Collections.Generic;

namespace TableForge.UI
{
    internal class XorFunction : ExcelFunctionBase
    {
        public override string Name => "XOR";
        protected override ArgumentDefinitionCollection ArgumentDefinitions { get; } = new ArgumentDefinitionCollection(new List<ArgumentDefinition>
        {
            new(ArgumentType.Boolean),
            new(ArgumentType.Boolean, true, true) 
        });
        
        public override object Evaluate(List<object> args, FunctionContext context)
        {
            int trueCount = 0;
            foreach (var arg in args)
            {
                if (FunctionArgumentHelper.ConvertToBoolean(arg))
                {
                    trueCount++;
                    if (trueCount > 1) // More than one true means XOR is false
                        return false;
                }
            }
            return trueCount == 1; // XOR is true only if exactly one argument is true
        }
    }
}