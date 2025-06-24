using System.Collections.Generic;

namespace TableForge.Editor.UI
{
    internal static class ExcelOperators
    {
        private static readonly List<string> _compareOperators = new()
        {
            "<=", ">=", "<>", "!=", "=", "<", ">" //Order matters: longest first
        };
        
        private static readonly List<string> _arithmeticOperators = new()
        {
            "+", "-", "*", "/", "%", "^"
        };
        
        public static IReadOnlyList<string> CompareOperators => _compareOperators;
        public static IReadOnlyList<string> ArithmeticOperators => _arithmeticOperators;
        
        public static string SkipOperator(string input, out string operatorUsed)
        {
            operatorUsed = string.Empty;

            if (string.IsNullOrEmpty(input))
                return input;

            foreach (var op in _compareOperators)
            {
                if (input.StartsWith(op))
                {
                    operatorUsed = op;
                    return input.Substring(op.Length).Trim();
                }
            }
            
            foreach (var op in _arithmeticOperators)
            {
                if (input.StartsWith(op))
                {
                    operatorUsed = op;
                    return input.Substring(op.Length).Trim();
                }
            }

            return input.Trim();
        }
        
        public static bool TryExtractArithmeticOperator(string input, out string operatorUsed)
        {
            operatorUsed = string.Empty;

            if (string.IsNullOrEmpty(input))
                return false;

            foreach (var op in _arithmeticOperators)
            {
                if (input.Contains(op))
                {
                    operatorUsed = op;
                    return true;
                }
            }

            return false;
        }
        
        public static bool TryExtractCompareOperator(string input, out string operatorUsed)
        {
            operatorUsed = string.Empty;

            if (string.IsNullOrEmpty(input))
                return false;

            foreach (var op in _compareOperators)
            {
                if (input.Contains(op))
                {
                    operatorUsed = op;
                    return true;
                }
            }

            return false;
        }
    }
}