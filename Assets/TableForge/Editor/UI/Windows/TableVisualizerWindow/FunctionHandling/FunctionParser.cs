using System;
using System.Collections.Generic;
using UnityEngine;

namespace TableForge.Editor.UI
{
    internal class FunctionParser
    {
        private readonly ArgumentParser _argumentParser = new();

        public static string OffsetFunction(string function, string originalPosition, string finalPosition, Table baseTable)
        {
            List<string> references = ReferenceParser.ExtractReferences(function);
            if (references.Count == 0) return function;

            string[] offsetReferences = new string[references.Count];
            try
            {
                for (var i = 0; i < references.Count; i++)
                {
                    offsetReferences[i] = ReferenceParser.GetRelativeReference(references[i], originalPosition, finalPosition, baseTable);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }

            for (int i = 0; i < references.Count; i++)
            {
                function = function.Replace(references[i], offsetReferences[i]);
            }
            
            return function;
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