using System;
using System.Collections.Generic;
using UnityEngine;

namespace TableForge.Editor.UI
{
    internal class Filterer
    {
        private readonly TableControl _tableControl;
        private readonly HashSet<int> _hiddenRows;

        public HashSet<int> HiddenRows => _hiddenRows;

        public Filterer(TableControl tableControl)
        {
            _tableControl = tableControl;
            _hiddenRows = new HashSet<int>();
        }
        
        public bool IsVisible(int rowId) => !_hiddenRows.Contains(rowId);

        public void Filter(string input)
        {
            ProcessInput(input);
            _tableControl.RebuildPage();
        }

        private void ProcessInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                ApplyFilter(row => true);
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
                ApplyFilter(row => true);
            }
        }

        private Func<Row, bool> ProcessExpression(string input)
        {
            var tokens = Tokenize(input);
            var parser = new ExpressionParser(tokens, _tableControl);
            return parser.Parse();
        }

        private List<string> Tokenize(string input)
        {
            var tokens = new List<string>();
            int i = 0;
            while (i < input.Length)
            {
                if (char.IsWhiteSpace(input[i]))
                {
                    i++;
                    continue;
                }

                if (input[i] == '(' || input[i] == ')')
                {
                    tokens.Add(input[i].ToString());
                    i++;
                    continue;
                }

                if (i + 1 < input.Length && 
                   (input.Substring(i, 2) == "&&" || input.Substring(i, 2) == "||"))
                {
                    tokens.Add(input.Substring(i, 2));
                    i += 2;
                    continue;
                }

                if (input[i] == '&' || input[i] == '|')
                {
                    tokens.Add(input[i].ToString());
                    i++;
                    continue;
                }

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

        private void ApplyFilter(Func<Row, bool> condition)
        {
            _hiddenRows.Clear();

            foreach (var row in _tableControl.TableData.OrderedRows)
            {
                if (!condition(row))
                    _hiddenRows.Add(row.Id);
            }
        }

    }
}