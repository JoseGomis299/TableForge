using System;
using System.Collections.Generic;
using System.Linq;

namespace TableForge.Editor.UI
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
            return _functions.GetValueOrDefault(name);
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
        
        public static int FindFunction(string input, out string functionName, FunctionReturnType returnType = FunctionReturnType.Any)
        {
            functionName = string.Empty;
            if (string.IsNullOrWhiteSpace(input))
                return -1;

            List<string> orderedFunctions = new List<string>(_functions.Keys.OrderByDescending(n => n.Length));
            List<(int Index, string name)> validFunctions = new List<(int, string)>();
            foreach (var function in orderedFunctions)
            {
                if (input.Contains(function, StringComparison.OrdinalIgnoreCase)
                    && (GetFunction(function).ReturnType & returnType) != 0)
                {
                    validFunctions.Add((input.IndexOf(function, StringComparison.OrdinalIgnoreCase), function));
                }
            }
            
            if (validFunctions.Count > 0)
            {
                // Get the first valid function based on the earliest index
                var firstValidFunction = validFunctions.OrderBy(vf => vf.Index).First();
                functionName = firstValidFunction.name;
                return firstValidFunction.Index;
            }

            return -1;
        }
    }
}