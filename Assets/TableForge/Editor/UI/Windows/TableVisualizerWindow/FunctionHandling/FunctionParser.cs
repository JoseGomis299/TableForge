using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace TableForge.Editor.UI
{
    internal class FunctionParser
    {
        private readonly ReferenceParser _referenceParser = new();
        private readonly FunctionExecutor _executor;

        public FunctionParser(FunctionExecutor executor)
        {
            _executor = executor;
        }

        public Action ParseCellFunction(string input, Cell cell)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            return () => ExecuteFunction(input, cell);
        }

        public Action ParseColumnFunction(string input, Column column)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            return () => ExecuteColumnFunction(input, column);
        }

        private void ExecuteFunction(string input, Cell cell)
        {
            var context = new FunctionContext(
                cell,
                _referenceParser,
                _executor
            );

            object result = null;
            try
            {
                result = EvaluateExpression(input, context);
                if (result == null)
                {
                    Debug.LogError($"Function evaluation failed for input: {input}");
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Function evaluation error for input: {input}\n" +
                               $"Error: {e.Message}");
                return;
            }

            try
            {
                object properTypeResult = Convert.ChangeType(result, cell.Type, CultureInfo.InvariantCulture);
                cell.SetValue(properTypeResult);
            }
            catch (Exception e)
            {
                Debug.LogError($"Function evaluation error for input: {input}\n" +
                               $"Expected type: {cell.Type}, but got: {result.GetType()}\n" +
                               $"Error: {e.Message}");
            }
        }

        private void ExecuteColumnFunction(string input, Column column)
        {
            foreach (var row in column.Table.OrderedRows)
            {
                if (row.Cells.TryGetValue(column.Position, out Cell cell))
                {
                    ExecuteFunction(input, cell);
                }
            }
        }

        private object EvaluateExpression(string expression, FunctionContext context)
        {
            if (expression.StartsWith("="))
                return EvaluateFunction(expression[1..], context);
            
            // Handle constants
            if (double.TryParse(expression, out double number))
                return number;
            
            return expression; // String constant
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

        private List<object> ParseArguments(string argsStr, FunctionContext context, IReadOnlyList<ArgumentDefinition> expectedArguments)
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
        
        private object ParseArgument(string token, FunctionContext context, ArgumentDefinition expectedArg)
        {
            if ((expectedArg.type & ArgumentType.Reference) != 0)
            {
                if(context.ReferenceParser.IsReference(token))
                    return context.ReferenceParser.ResolveReference(token, context.ContextCell);
                
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
                    if (ExcelOperators.compareOperators.Any(token.StartsWith))
                    {
                        right = ExcelOperators.SkipOperator(token, out op);
                    }
                    
                    return ConditionEvaluator.Evaluate(op, right);
                }
            }

            if ((expectedArg.type & ArgumentType.ValueFunction) != 0)
            {
                if (FunctionRegistry.StringContainsFunction(token) && token.Contains('(') && token.EndsWith(")"))
                {
                    return EvaluateFunction(token, context);
                }
                
                bool hasMoreFlags = (expectedArg.type & ~ArgumentType.ValueFunction) != 0;
                if (!hasMoreFlags)
                    return null; // Not a valid function argument
            }
            
            if((expectedArg.type & ArgumentType.LogicalFunction) != 0)
            {
                if (FunctionRegistry.StringContainsFunction(token, FunctionReturnType.Boolean) && token.Contains('(') && token.EndsWith(")"))
                {
                    return EvaluateFunction(token, context);
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