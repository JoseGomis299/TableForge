using System.Collections.Generic;

namespace TableForge.UI
{
    internal static class ExcelOperators
    {
        public static List<string> CompareOperators = new List<string>
        {
            "<=", ">=", "<>", "!=", "=", "<", ">" //Order matters: longest first
        };
        
        public static string SkipOperator(string input, out string operatorUsed)
        {
            operatorUsed = string.Empty;

            if (string.IsNullOrEmpty(input))
                return input;

            foreach (var op in CompareOperators)
            {
                if (input.StartsWith(op))
                {
                    operatorUsed = op;
                    return input.Substring(op.Length).Trim();
                }
            }

            return input.Trim();
        }
    }
}