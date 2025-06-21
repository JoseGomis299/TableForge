using System;
using System.Collections.Generic;

namespace TableForge.UI
{
    internal class AbsFunction : ExcelFunctionBase
    {
        public override string Name => "ABS";
        protected override ArgumentDefinitionCollection ArgumentDefinitions { get; } = new ArgumentDefinitionCollection(new List<ArgumentDefinition>
        {
            new(ArgumentType.Number)
        });
        
        public override object Evaluate(List<object> args, FunctionContext context)
        {
            if (FunctionArgumentHelper.TryGetSingleNumber(args[0], out double number))
            {
                return Math.Abs(number);
            }
            
            throw new ArgumentException("ABS function requires a numeric argument.");
        }
    }
}