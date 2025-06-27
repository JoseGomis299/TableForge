using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace TableForge.Editor.UI
{
    internal class ArgumentParser
    {
        public List<object> ParseArguments(string argsStr, FunctionContext context, IReadOnlyList<ArgumentDefinition> expectedArguments)
        {
            var args = new List<object>();
            var argTokens = SplitArguments(argsStr);

            int i = 0;
            foreach (var token in argTokens)
            {
                int argIndex = Math.Min(i, expectedArguments.Count - 1);
                ArgumentDefinition expectedArgument = expectedArguments[argIndex];
                
                args.Add(ParseArgument(token, context, expectedArgument));
                i++;
            }
            
            return args;
        }
        
        public object ParseArgument(string token, FunctionContext context, ArgumentDefinition expectedArg)
        {
            token = token.Trim();
            
            if ((expectedArg.type & ArgumentType.Reference) != 0)
            {
                if(ReferenceParser.IsReference(token))
                    return ReferenceParser.ResolveReference(token, context.BaseTable);
                
                bool hasMoreFlags = (expectedArg.type & ~ArgumentType.Reference) != 0;
                if (!hasMoreFlags)
                    return null; // Not a valid reference argument
            }
            
            if ((expectedArg.type & ArgumentType.Numeric) != 0)
            {
                if (token.TryParseNumber(out double number))
                    return number.ToString(CultureInfo.InvariantCulture);
                
                bool hasMoreFlags = (expectedArg.type & ~ArgumentType.Numeric) != 0;
                if (!hasMoreFlags)
                    return null; // Not a valid numeric argument
            }
            
            if ((expectedArg.type & ArgumentType.Text) != 0)
            {
                bool hasMoreFlags = (expectedArg.type & ~ArgumentType.Text) != 0;
                if (!token.StartsWith("\"") || !token.EndsWith("\""))
                {
                    if (!hasMoreFlags)
                        return null; // Not a valid text argument
                }
                else
                {
                    token = token[1..^1]; // Remove quotes
                    if(expectedArg.type.HasFlag(ArgumentType.String))
                        return token; 
                    
                    // If it is not a string it must be a criteria
                    string op = "=", right = token;
                    if (ExcelOperators.CompareOperators.Any(token.StartsWith))
                    {
                        right = ExcelOperators.SkipOperator(token, out op);
                    }
                    
                    return ConditionEvaluator.Evaluate(op, right);
                }
            }
            
            if ((expectedArg.type & ArgumentType.ArithmeticOperation) != 0)
            {
                if (FunctionInputResolver.IsArithmeticOperation(token))
                {
                    List<string> splitTokens = FunctionInputResolver.TokenizeArithmeticOperation(token);

                    if (splitTokens.Count > 1)
                    {
                        double result = new ArithmeticResolver(this).Evaluate(splitTokens.ToList(), context);
                        return result;
                    }
                }
                
                bool hasMoreFlags = (expectedArg.type & ~ArgumentType.ArithmeticOperation) != 0;
                if (!hasMoreFlags)
                    return null; // Not a valid arithmetic operation argument
            }

            if ((expectedArg.type & ArgumentType.StringFunction) != 0)
            {
                if (FunctionInputResolver.TryExtractFunction(token, out string func, FunctionReturnType.String))
                {
                    return EvaluateFunction(func, context);
                }
                
                bool hasMoreFlags = (expectedArg.type & ~ArgumentType.StringFunction) != 0;
                if (!hasMoreFlags)
                    return null; // Not a valid string function argument
            }
            
            if ((expectedArg.type & ArgumentType.NumericFunction) != 0)
            {
                if (FunctionInputResolver.TryExtractFunction(token, out string func, FunctionReturnType.Number))
                {
                    return EvaluateFunction(func, context);
                }
                
                bool hasMoreFlags = (expectedArg.type & ~ArgumentType.NumericFunction) != 0;
                if (!hasMoreFlags)
                    return null; // Not a valid number function argument
            }
            
            if((expectedArg.type & ArgumentType.LogicalFunction) != 0)
            {
                if (FunctionInputResolver.TryExtractFunction(token, out string func, FunctionReturnType.Boolean))
                {
                    return EvaluateFunction(func, context);
                }
                
                bool hasMoreFlags = (expectedArg.type & ~ArgumentType.LogicalFunction) != 0;
                if (!hasMoreFlags)
                    return null; // Not a valid logical function argument
            }
            
            if ((expectedArg.type & ArgumentType.LogicExpression) != 0)
            {
                if(token.Equals("true", StringComparison.OrdinalIgnoreCase))
                    return true;
                if(token.Equals("false", StringComparison.OrdinalIgnoreCase))
                    return false;
                
                int depth = 0, operatorIndex = 0, operatorLength = 0;
                
                for (int i = 0; i < token.Length; i++)
                {
                    char c = token[i];
                    switch (c)
                    {
                        case '(': depth++; break;
                        case ')': depth--; break;
                        case '=' or '<' or '>' or '!' when depth == 0:
                            operatorIndex = i;
                            operatorLength = 1;
                            if(token[i+1] is '=' or '>' or '<')
                            {
                                operatorLength++;
                            }
                            break;
                    }
                }

                bool hasMoreFlags = (expectedArg.type & ~ArgumentType.LogicExpression) != 0;
                if (operatorIndex == 0 || depth != 0)
                {
                    if (!hasMoreFlags)
                        return null; // Not a valid logic expression argument
                }
                else
                {
                    string left = token.Substring(0, operatorIndex).Trim();
                    string op = token.Substring(operatorIndex, operatorLength).Trim();
                    string right = token.Substring(operatorIndex + operatorLength).Trim();
                
                    ArgumentDefinition expectedSubArg = new ArgumentDefinition(ArgumentType.Number | ArgumentType.Boolean, "");
                    object leftValue = ParseArgument(left, context, expectedSubArg);
                    object rightValue = ParseArgument(right, context, expectedSubArg);
                    
                    if (leftValue == null || rightValue == null)
                    {
                        if (!hasMoreFlags)
                            return null; // No valid arguments for logic expression
                    }

                    try
                    {
                        return ConditionEvaluator.Evaluate(leftValue, op, rightValue);
                    }
                    catch (Exception)
                    {
                        if (!hasMoreFlags)
                            return null; // Not a valid logic expression argument
                    }
                }
            }

            return null; // If we reach here, the argument is not valid for the expected type
        }
        
        private object EvaluateFunction(string expression, FunctionContext context)
        {
            // Match function pattern: NAME(ARG1; ARG2; ...)
            var match = Regex.Match(expression, @"^(\w+)\((.*)\)$");
            if (!match.Success)
                return expression; // Not a function, treat as reference or constant

            string funcName = match.Groups[1].Value;
            string argsStr = match.Groups[2].Value;
            
            var function = FunctionRegistry.GetFunction(funcName);
            var args = ParseArguments(argsStr, context, function.ExpectedArguments.Definitions);

            if(!function.ValidateArguments(args))
            {
                Debug.LogError($"Invalid arguments for function '{funcName}'");
                return null; // Invalid function call
            }
            
            return function.Evaluate(args, context);
        }

        private IEnumerable<string> SplitArguments(string argsStr)
        {
            int depth = 0;
            int start = 0;
            
            for (int i = 0; i < argsStr.Length; i++)
            {
                char c = argsStr[i];
                switch (c)
                {
                    case '(': depth++; break;
                    case ')': depth--; break;
                    case ';' when depth == 0:
                        yield return argsStr.Substring(start, i - start).Trim();
                        start = i + 1;
                        break;
                }
            }
            yield return argsStr.Substring(start).Trim();
        }
    }
}