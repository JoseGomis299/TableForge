using System;
using System.Collections.Generic;

namespace TableForge.UI
{
    internal static class FunctionRegistry
    {
        private static readonly Dictionary<string, IExcelFunction> Functions = 
            new Dictionary<string, IExcelFunction>(StringComparer.OrdinalIgnoreCase);

        static FunctionRegistry()
        {
            RegisterFunction(new SumFunction());
            RegisterFunction(new IfFunction());
            RegisterFunction(new SumIfFunction());
            RegisterFunction(new CountFunction());
            RegisterFunction(new CountIfFunction());
            RegisterFunction(new MinFunction());
            RegisterFunction(new MaxFunction());
            RegisterFunction(new AverageFunction());
            RegisterFunction(new OrFunction());
            RegisterFunction(new NotFunction());
            RegisterFunction(new AndFunction());
            RegisterFunction(new XorFunction());
            RegisterFunction(new AbsFunction());
            RegisterFunction(new RoundFunction());
            RegisterFunction(new ModFunction());
            RegisterFunction(new DivideFunction());
            RegisterFunction(new MultiplyFunction());
        }

        public static void RegisterFunction(IExcelFunction function)
        {
            Functions[function.Name] = function;
        }

        public static IExcelFunction GetFunction(string name)
        {
            if (Functions.TryGetValue(name, out var function))
                return function;

            throw new KeyNotFoundException($"Function '{name}' is not supported.");
        }
        
        public static bool StringContainsFunction(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            foreach (var function in Functions.Keys)
            {
                if (input.Contains(function, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}