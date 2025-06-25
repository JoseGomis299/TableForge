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

                if (result is List<Cell> list)
                {
                    if(list.Count > 1)
                    {
                        Debug.LogError($"Function evaluation returned multiple cells for input: {input}");
                        return;
                    }

                    if (list.Count == 1)
                    {
                        result = list[0].GetValue();
                    }
                    else
                    {
                        Debug.LogError($"Function evaluation returned an invalid reference: {input}");
                        return;
                    }
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
                return _argumentParser.ParseArgument(expression[1..], context, new ArgumentDefinition(ArgumentType.Any & ~ArgumentType.Criteria, ""));
            
            // Handle constants
            if (expression.TryParseNumber(out double number))
                return number;
            
            return expression; // String constant
        }
    }
}