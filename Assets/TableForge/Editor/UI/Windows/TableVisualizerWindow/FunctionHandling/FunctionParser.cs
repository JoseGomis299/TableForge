using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace TableForge.Editor.UI
{
    internal class FunctionParser
    {
        private readonly ReferenceParser _referenceParser = new();
        private readonly FunctionExecutor _executor;
        private readonly ArgumentParser _argumentParser = new();

        public FunctionParser(FunctionExecutor executor)
        {
            _executor = executor;
        }

        public Func<object> ParseCellFunction(string input, Table baseTable)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            return () => ExecuteFunction(input, baseTable);
        }

        private object ExecuteFunction(string input, Table baseTable)
        {
            var context = new FunctionContext(
                baseTable,
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
                    return null;
                }

                if (result is List<Cell> list)
                {
                    if(list.Count > 1)
                    {
                        Debug.LogError($"Function evaluation returned multiple cells for input: {input}");
                        return null;
                    }

                    if (list.Count == 1)
                    {
                        result = list[0].GetValue();
                    }
                    else
                    {
                        Debug.LogError($"Function evaluation returned an invalid reference: {input}");
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Function evaluation error for input: {input}\n" +
                               $"Error: {e.Message}");
                return null;
            }

            return result;
        }

        private object EvaluateExpression(string expression, FunctionContext context)
        {
            if (expression.StartsWith("="))
                return _argumentParser.ParseArgument(expression[1..], context, new ArgumentDefinition(ArgumentType.Any & ~ArgumentType.Criteria, ""));
            
            // Handle constants
            if (expression.TryParseNumber(out double number))
                return number;
            
            return expression; // String constant
        }
    }
}