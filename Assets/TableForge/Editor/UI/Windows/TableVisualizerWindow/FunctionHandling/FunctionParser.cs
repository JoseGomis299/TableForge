using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace TableForge.Editor.UI
{
    internal class FunctionParser
    {
        private readonly ArgumentParser _argumentParser = new();

        public Func<object> ParseCellFunction(string input, Table baseTable)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            return () => ExecuteFunction(input, baseTable);
        }

        private object ExecuteFunction(string input, Table baseTable)
        {
            var context = new FunctionContext(
                baseTable
            );

            object result = null;
            result = EvaluateExpression(input, context);
            if (result == null)
            {
                throw new InvalidOperationException($"Invalid result.");
            }

            if (result is List<Cell> list)
            {
                if(list.Count > 1)
                {
                    throw new InvalidOperationException($"Function evaluation returned multiple cells for input: {input}");
                }

                if (list.Count == 1)
                {
                    result = list[0].GetValue();
                }
                else
                {
                    throw new InvalidOperationException($"Function evaluation returned an empty list for input: {input}");
                }
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