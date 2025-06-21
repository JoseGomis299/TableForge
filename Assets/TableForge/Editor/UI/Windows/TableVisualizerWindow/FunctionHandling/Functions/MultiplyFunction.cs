using System;
using System.Collections.Generic;

namespace TableForge.UI
{
    internal class MultiplyFunction : ExcelFunctionBase
    {
        public override string Name => "MULTIPLY";
        protected override ArgumentDefinitionCollection ArgumentDefinitions { get; } = new ArgumentDefinitionCollection(new List<ArgumentDefinition>
        {
            new(ArgumentType.Number),
            new(ArgumentType.Number)
        });
        
        public override object Evaluate(List<object> args, FunctionContext context)
        {
            if (FunctionArgumentHelper.TryGetSingleNumber(args[0], out double a) && 
                FunctionArgumentHelper.TryGetSingleNumber(args[1], out double b))
            {
                return a * b;
            }
            
            throw new ArgumentException("MULTIPLY function requires numeric arguments.");
        }
    }
}