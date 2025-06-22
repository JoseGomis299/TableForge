using System;
using System.Collections.Generic;
using UnityEngine;

namespace TableForge.UI
{
    internal static class FunctionRegistry
    {
        private static readonly Dictionary<string, IExcelFunction> _functions = new(StringComparer.OrdinalIgnoreCase);

        static FunctionRegistry()
        {
            //Register all functions that implement IExcelFunction
            var functionTypes = typeof(FunctionRegistry).Assembly.GetTypes();
            foreach (var type in functionTypes)
            {
                if (typeof(IExcelFunction).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    var function = (IExcelFunction)Activator.CreateInstance(type);
                    RegisterFunction(function);
                }
            }
        }

        public static void RegisterFunction(IExcelFunction function)
        {
            _functions[function.Name] = function;
        }

        public static IExcelFunction GetFunction(string name)
        {
            if (_functions.TryGetValue(name, out var function))
                return function;

            throw new KeyNotFoundException($"Function '{name}' is not supported.");
        }
        
        public static bool StringContainsFunction(string input, FunctionReturnType returnType = FunctionReturnType.Any)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            foreach (var function in _functions.Keys)
            {
                if (input.Contains(function, StringComparison.OrdinalIgnoreCase)
                    && (_functions[function].ReturnType & returnType) != 0)
                    return true;
            }

            return false;
        }
    }
}