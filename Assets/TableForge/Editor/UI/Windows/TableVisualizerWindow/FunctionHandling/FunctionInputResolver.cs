using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TableForge.Editor.UI
{
    internal static class FunctionInputResolver
    {
        public static bool TryExtractFunction(string input, out string func, FunctionReturnType functionType = FunctionReturnType.Any)
        {
            func = string.Empty;
            if (string.IsNullOrEmpty(input))
                return false;

            input = input.Trim();
            List<string> tokens = TokenizeByFunction(input);
            foreach (var token in tokens)
            {
                bool containsFunction = FunctionRegistry.FindFunction(token, out string functionName) != -1;
                if (!containsFunction && ExcelOperators.TryExtractArithmeticOperator(token, out _))
                {
                    return false;
                }   
                
                if (containsFunction)
                {
                    IExcelFunction function = FunctionRegistry.GetFunction(functionName);
                    if ((function.ReturnType & functionType) == 0)
                        return false;

                    func = token.Trim();
                    return true;
                }
            }
            
            return false;
        }
        
        public static bool IsArithmeticOperation(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            input = input.Trim();
            List<string> tokens = TokenizeByFunction(input);
            for (var i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                bool containsFunction = FunctionRegistry.StringContainsFunction(token);
                if (!containsFunction && ExcelOperators.TryExtractArithmeticOperator(token, out _))
                {
                    return true;
                }
            }

            return false;
        }
        
        public static List<string> TokenizeArithmeticOperation(string input)
        {
            if (string.IsNullOrEmpty(input))
                return new List<string>();

            List<string> tokens = TokenizeByFunction(input);
            List<string> arithmeticTokens = new List<string>();
            foreach (var token in tokens)
            {
                if (FunctionRegistry.StringContainsFunction(token))
                {
                    arithmeticTokens.Add(token.Trim());
                }
                else
                {
                    arithmeticTokens.AddRange(Regex.Split(token, @"([()+\-*/%\^])").Where(s => !string.IsNullOrEmpty(s.Trim())));
                }
            }

            return arithmeticTokens;
        }
     
        private static List<string> TokenizeByFunction(string input)
        {
            List<string> tokens = new List<string>();
            int startIndex;
            do
            {
                startIndex = FunctionRegistry.FindFunction(input, out _);
                if (startIndex != -1)
                {
                    string startSubstring = input.Substring(0, startIndex).Trim();
                    if(!startSubstring.Equals("()") && !string.IsNullOrEmpty(startSubstring.Trim()))
                    {
                        tokens.Add(startSubstring);
                    }
                    
                    input = input.Substring(startIndex);
                    tokens.Add(ExtractFirstFunction(input, out int endIndex));
                    
                    if (endIndex == -1)
                        throw new ArgumentException("Invalid function format in input string.");
                    input = input.Substring(endIndex);
                }
            } while (startIndex != -1);

            if (!string.IsNullOrEmpty(input.Trim()))
                tokens.Add(input.Trim());

            return tokens;
        }
        
        private static string ExtractFirstFunction(string input, out int endIndex)
        {
            endIndex = -1;
            if (string.IsNullOrEmpty(input))
                return string.Empty;
            
            int depth = 0;
            string currentToken = string.Empty;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                currentToken += c;

                if (c == '(')
                {
                    depth++;
                }
                else if (c == ')')
                {
                    depth--;
                    if (depth < 0)
                        throw new ArgumentException("Unmatched closing parenthesis in function input.");
                        
                    if (depth == 0)
                    {
                        endIndex = i + 1; // Include the closing parenthesis
                        return currentToken.Trim();
                    }
                }
            }
            
            if (depth != 0)
                throw new ArgumentException("Unmatched opening parenthesis in function input.");
            
            return string.Empty; // No function found
        }
    }
}