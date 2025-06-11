using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;

namespace TableForge.UI
{
    internal class ExpressionParser
    {
        private readonly List<string> _tokens;
        private int _position;
        private readonly TableControl _tableControl;

        public ExpressionParser(List<string> tokens, TableControl tableControl)
        {
            _tokens = tokens;
            _tableControl = tableControl;
            _position = 0;
        }

        public Func<Row, bool> Parse()
        {
            return ParseOr();
        }

        private Func<Row, bool> ParseOr()
        {
            var left = ParseAnd();
            while (Match("||") || Match("|"))
            {
                var right = ParseAnd();
                left = CombineOr(left, right);
            }
            return left;
        }

        private Func<Row, bool> ParseAnd()
        {
            var left = ParseTerm();
            while (Match("&&") || Match("&"))
            {
                var right = ParseTerm();
                left = CombineAnd(left, right);
            }
            return left;
        }

        private Func<Row, bool> ParseTerm()
        {
            if (Match("("))
            {
                var expr = ParseOr();
                Expect(")");
                return expr;
            }

            if (Match("!"))
            {
                var term = ParseTerm();
                return row => !term(row);
            }

            var token = NextToken();
            if (IsFilterToken(token))
            {
                return CreateFilter(token);
            }

            throw new Exception($"Unexpected token: {token}");
        }

        private Func<Row, bool> CreateFilter(string token)
        {
            var parts = token.Split(new[] { ':' }, 2);
            if (parts.Length != 2) 
                return row => true;

            var identifier = parts[0].ToLower();
            var value = parts[1];

            return identifier switch
            {
                "g" or "guid" => CreateGuidFilter(value),
                "path" => CreatePathFilter(value),
                "n" or "name" => CreateNameFilter(value),
                "p" or "property" => CreatePropertyFilter(value),
                _ => row => true
            };
        }

        private Func<Row, bool> CreateGuidFilter(string guid)
        {
            return row => 
                row.SerializedObject?.RootObjectGuid?.Equals(guid, StringComparison.OrdinalIgnoreCase) == true;
        }

        private Func<Row, bool> CreatePathFilter(string path)
        {
            string fullPath = path.StartsWith("Assets/") ? path : "Assets/" + path;
            return row => 
                AssetDatabase.GetAssetPath(row.SerializedObject.RootObject)?.StartsWith(fullPath, StringComparison.OrdinalIgnoreCase) == true;
        }

        private Func<Row, bool> CreateNameFilter(string name)
        {
            return row => row.Name.Contains(name, StringComparison.OrdinalIgnoreCase);
        }

        private Func<Row, bool> CreatePropertyFilter(string condition)
        {
            var match = Regex.Match(condition, @"([\w\$\. ]+)\s*(==?|!=|>=|<=|>|<|~=|!~|=~|~!)\s*(.+)");
            if (!match.Success) 
                return row => true;

            var left = match.Groups[1].Value.Trim();
            var op = match.Groups[2].Value.Trim();
            var right = match.Groups[3].Value.Trim();

            return row =>
            {
                try
                {
                    var leftVal = GetCellValue(row, left) ?? left; 
                    var rightVal = GetCellValue(row, right) ?? right; 

                    return Compare(leftVal, op, rightVal);
                }
                catch
                {
                    return false;
                }
            };
        }

        private object GetCellValue(Row row, string columnRef)
        {
            if (string.IsNullOrEmpty(columnRef) || char.IsDigit(columnRef[0]) || columnRef[0] == '\"' || columnRef[0] == '\'')
                return null;
            
            Column column; Cell cell;
            //Nested column reference handling
            if (columnRef.Contains('.'))
            {
                List<object> values = new List<object>();
                var parts = columnRef.Split('.');
                
                column = GetColumn(parts[0], _tableControl.TableData);
                if (column == null) return null;
                cell = row.Cells[column.Position];

                if (cell is SubTableCell subTableCell)
                {
                    if (!RetrieveNestedValues(subTableCell.SubTable, parts, 1, values))
                        return null;

                    if (values.Count == 0) return null;
                    if (values.Count == 1) return values[0];
                    return values; // Return all values if multiple found
                }
                
                return null;
            }
            
            //Single column reference handling
            column = GetColumn(columnRef, _tableControl.TableData);
            if (column == null) 
                return null;

            cell = row.Cells[column.Position];
            return cell?.GetValue();
        }

        private bool RetrieveNestedValues(Table table, string[] parts, int index, List<object> values)
        {
            var part = parts[index];
            Column column = GetColumn(part, table);
            if (column == null)
                return false;

            for (int i = 1; i <= table.Rows.Count; i++)
            {
                Cell cell = table.GetCell(column.Position, i);
                if (cell == null) return false;
                if(index == parts.Length - 1)
                {
                    values.Add(cell.GetValue());
                }
                else
                {
                    if (cell is SubTableCell subTableCell)
                    {
                        if (!RetrieveNestedValues(subTableCell.SubTable, parts, index + 1, values))
                            return false;
                    }
                    else
                    {
                        return false; // Not a sub-table cell
                    }
                }
            }
            
            return true;
        }

        private Column GetColumn(string reference, Table table)
        {
            if (reference.StartsWith("$") && reference.Length == 2 && char.IsLetter(reference[1]))
            {
                return table.Columns.GetValueOrDefault(PositionUtil.ConvertToNumber(reference[1].ToString()));
            }

            return table.ColumnsByName.GetValueOrDefault(reference);
        }

        private bool Compare(object left, string op, object right)
        {
            if (left == null || right == null) 
                return false;
            
            // List comparison
            if (left is IList<object> leftList && right is IList<object> rightList)
            {
                return op switch
                {
                    "=" or "==" => leftList.SequenceEqual(rightList),
                    "!=" => !leftList.SequenceEqual(rightList),
                    ">" => leftList.Count > rightList.Count,
                    "<" => leftList.Count < rightList.Count,
                    ">=" => leftList.Count >= rightList.Count,
                    "<=" => leftList.Count <= rightList.Count,
                    "~=" or "=~" => rightList.All(item => leftList.Contains(item)),
                    "!~" or "~!" => rightList.Any(item => !leftList.Contains(item)),
                    _ => false
                };
            }
            if (left is IList<object> leftValues)
            {
                return op switch
                {
                    "~=" or "=~" => leftValues.Contains(right),
                    "!~" or "~!" => !leftValues.Contains(right),
                    "=" or "==" => leftValues.Count == 1 && leftValues[0].Equals(right),
                    "!=" => leftValues.Count != 1 || !leftValues[0].Equals(right),
                    _ => false
                };
            }
            if (right is IList<object> rightValues)
            {
                return op switch
                {
                    "~=" or "=~" => rightValues.Contains(left),
                    "!~" or "~!" => !rightValues.Contains(left),
                    "=" or "==" => rightValues.Count == 1 && rightValues[0].Equals(left),
                    "!=" => rightValues.Count != 1 || !rightValues[0].Equals(left),
                    _ => false
                };
            }

            // Numerical comparison
            if (TryParseNumber(left, out double leftNum) && TryParseNumber(right, out double rightNum))
            {
                return op switch
                {
                    "=" or "==" => Math.Abs(leftNum - rightNum) < double.Epsilon,
                    "!=" => Math.Abs(leftNum - rightNum) > double.Epsilon,
                    ">" => leftNum > rightNum,
                    "<" => leftNum < rightNum,
                    ">=" => leftNum >= rightNum,
                    "<=" => leftNum <= rightNum,
                    _ => false
                };
            }

            // String comparison
            string leftStr = left.ToString();
            string rightStr = right.ToString();

            return op switch
            {
                "=" or "==" => leftStr.Equals(rightStr, StringComparison.OrdinalIgnoreCase),
                "!=" => !leftStr.Equals(rightStr, StringComparison.OrdinalIgnoreCase),
                "~=" or "=~" => leftStr.Contains(rightStr),
                "!~" or "~!" => !leftStr.Contains(rightStr),
                _ => false
            };
        }

        private bool TryParseNumber(object value, out double result)
        {
            result = 0;
            if (value == null) 
                return false;

            return double.TryParse(value.ToString(), out result);
        }

        private bool IsFilterToken(string token)
        {
            return token.Contains(":");
        }

        private bool Match(string expected)
        {
            if (_position < _tokens.Count && _tokens[_position] == expected)
            {
                _position++;
                return true;
            }
            return false;
        }

        private void Expect(string expected)
        {
            if (!Match(expected))
                throw new Exception($"Expected '{expected}'");
        }

        private string NextToken()
        {
            if (_position >= _tokens.Count)
                throw new Exception("Unexpected end of expression");
            return _tokens[_position++];
        }

        private Func<Row, bool> CombineAnd(Func<Row, bool> left, Func<Row, bool> right)
        {
            return row => left(row) && right(row);
        }

        private Func<Row, bool> CombineOr(Func<Row, bool> left, Func<Row, bool> right)
        {
            return row => left(row) || right(row);
        }
    }
}