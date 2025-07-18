using System;
using System.Collections.Generic;
using UnityEngine;

namespace TableForge.Editor.UI
{
    /// <summary>
    /// Handles filtering of table rows based on user input expressions.
    /// Supports complex filtering expressions with logical operators and cell value comparisons.
    /// </summary>
    internal class Filterer
    {
        #region Private Fields

        private readonly TableControl _tableControl;
        private readonly HashSet<int> _hiddenRows;

        #endregion

        #region Public Properties

        public HashSet<int> HiddenRows => _hiddenRows;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the Filterer class.
        /// </summary>
        /// <param name="tableControl">The table control to filter.</param>
        public Filterer(TableControl tableControl)
        {
            _tableControl = tableControl;
            _hiddenRows = new HashSet<int>();
        }

        #endregion

        #region Public Methods

        public bool IsVisible(int rowId) => !_hiddenRows.Contains(rowId);

        /// <summary>
        /// Applies a filter expression to the table and rebuilds the page.
        /// </summary>
        /// <param name="input">The filter expression to apply.</param>
        public void Filter(string input)
        {
            ProcessInput(input);
            _tableControl.RebuildPage();
        }

        #endregion

        #region Private Methods - Input Processing

        /// <summary>
        /// Processes the input filter expression and applies it to the table.
        /// </summary>
        /// <param name="input">The filter expression to process.</param>
        private void ProcessInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                ApplyFilter(_ => true);
                return;
            }

            try
            {
                var expression = ProcessExpression(input);
                ApplyFilter(expression);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error applying filter: {e.Message}");
                ApplyFilter(_ => true);
            }
        }

        /// <summary>
        /// Processes a filter expression and returns a predicate function.
        /// </summary>
        /// <param name="input">The filter expression to process.</param>
        /// <returns>A predicate function that evaluates whether a row should be visible.</returns>
        private Func<Row, bool> ProcessExpression(string input)
        {
            var tokens = Tokenize(input);
            var parser = new ExpressionParser(tokens, _tableControl);
            return parser.Parse();
        }

        #endregion

        #region Private Methods - Tokenization

        /// <summary>
        /// Tokenizes the input string into individual tokens for parsing.
        /// </summary>
        /// <param name="input">The input string to tokenize.</param>
        /// <returns>A list of tokens representing the filter expression.</returns>
        private List<string> Tokenize(string input)
        {
            var tokens = new List<string>();
            int i = 0;
            
            while (i < input.Length)
            {
                // Skip whitespace
                if (char.IsWhiteSpace(input[i]))
                {
                    i++;
                    continue;
                }

                // Handle parentheses
                if (input[i] == '(' || input[i] == ')')
                {
                    tokens.Add(input[i].ToString());
                    i++;
                    continue;
                }

                // Handle logical operators (&&, ||)
                if (i + 1 < input.Length && 
                   (input.Substring(i, 2) == "&&" || input.Substring(i, 2) == "||"))
                {
                    tokens.Add(input.Substring(i, 2));
                    i += 2;
                    continue;
                }

                // Handle single logical operators (&, |)
                if (input[i] == '&' || input[i] == '|')
                {
                    tokens.Add(input[i].ToString());
                    i++;
                    continue;
                }

                // Extract other tokens (column names, values, operators)
                int start = i;
                while (i < input.Length && 
                       input[i] != '(' && input[i] != ')' && 
                       input[i] != '&' && input[i] != '|')
                {
                    i++;
                }
                
                tokens.Add(input.Substring(start, i - start).Trim());
            }
            
            return tokens;
        }

        #endregion

        #region Private Methods - Filter Application

        /// <summary>
        /// Applies a filter condition to all rows in the table.
        /// </summary>
        /// <param name="condition">The condition function that determines row visibility.</param>
        private void ApplyFilter(Func<Row, bool> condition)
        {
            _hiddenRows.Clear();

            foreach (var row in _tableControl.TableData.OrderedRows)
            {
                if (!condition(row))
                    _hiddenRows.Add(row.Id);
            }
        }

        #endregion
    }
}