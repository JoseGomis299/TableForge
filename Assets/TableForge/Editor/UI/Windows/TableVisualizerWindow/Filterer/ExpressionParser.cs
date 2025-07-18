using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using TableForge.Editor.UI.TableForge.UI;
using UnityEditor;
using Object = UnityEngine.Object;

namespace TableForge.Editor.UI
{
    internal class ExpressionParser
    {
        private readonly List<string> _tokens;
        private readonly TableControl _tableControl;
        private readonly ExpressionValueResolver _resolver;

        private int _position;

        public ExpressionParser(List<string> tokens, TableControl tableControl)
        {
            _tokens = tokens;
            _tableControl = tableControl;
            _position = 0;
            _resolver = new ExpressionValueResolver(tableControl);
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
            if (token.Contains(":"))
            {
                return CreateFilter(token);
            }

            throw new Exception($"Unexpected token: {token}");
        }

        private Func<Row, bool> CreateFilter(string token)
        {
            var parts = token.Split(new[] { ':' }, 2);
            if (parts.Length != 2) 
                return _ => true;

            var identifier = parts[0].ToLower();
            var value = parts[1];

            return identifier switch
            {
                "g" or "guid" => CreateGuidFilter(value),
                "path" => CreatePathFilter(value),
                "n" or "name" => CreateNameFilter(value),
                "p" or "property" => CreatePropertyFilter(value),
                _ => _ => true
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
            {
                string assetPath = AssetDatabase.GetAssetPath(row.SerializedObject.RootObject);
                if (string.IsNullOrEmpty(assetPath)) return false;

                return assetPath.StartsWith(fullPath);
            };
        }

        private Func<Row, bool> CreateNameFilter(string name)
        {
            return row => row.Name.Contains(name, StringComparison.OrdinalIgnoreCase);
        }

        private Func<Row, bool> CreatePropertyFilter(string condition)
        {
            var match = Regex.Match(condition, @"([\w\$\. \[\],]+)\s*(==?|<>|!=|>=|<=|>|<|~=|!~|=~|~!)\s*([\w\$\. \[\],]+)");
            if (!match.Success) 
                return _ => true;

            var left = match.Groups[1].Value.Trim();
            var op = match.Groups[2].Value.Trim();
            var right = match.Groups[3].Value.Trim();

            return row =>
            {
                try
                {
                    var leftVal = _resolver.GetCellValue(row, left) ?? _resolver.GetListValue(left);
                    var rightVal = _resolver.GetCellValue(row, right) ?? _resolver.GetListValue(right);

                    return Compare(leftVal, op, rightVal);
                }
                catch
                {
                    return false;
                }
            };
        }
        
        private bool Compare(object left, string op, object right)
        {
            if (left == null || right == null) 
                return false;
            
            if(left is Object obj) left = obj.name;
            if(right is Object obj2) right = obj2.name;
            
            // List comparison
            if (left is IList lList && right is IList rList)
            {
                var leftList = new List<string>();
                foreach (var item in lList)
                {
                    leftList.Add(item.ToString());
                }
                var rightList = new List<string>();
                foreach (var item in rList)
                {
                    rightList.Add(item.ToString());
                }
                
                return op switch
                {
                    "=" or "==" => leftList.SequenceEqual(rightList),
                    "!=" or "<>" => !leftList.SequenceEqual(rightList),
                    ">" => leftList.Count > rightList.Count,
                    "<" => leftList.Count < rightList.Count,
                    ">=" => leftList.Count >= rightList.Count,
                    "<=" => leftList.Count <= rightList.Count,
                    "~=" or "=~" => rightList.All(rItem => leftList.Any(lItem => lItem == rItem)),
                    "!~" or "~!" => rightList.Any(rItem => leftList.All(lItem => lItem != rItem)),
                    _ => false
                };
            }
            if (left is IList lv)
            {
                var leftValues = new List<string>();
                foreach (var item in lv)
                {
                    leftValues.Add(item.ToString());
                }
                
                if (right.TryParseNumber(out double rightNumber))
                {
                    return op switch
                    {
                        "~=" or "=~" => leftValues.Any(item => item == right.ToString()),
                        "!~" or "~!" => leftValues.All(item => item != right.ToString()),
                        "=" or "==" => Math.Abs(rightNumber - leftValues.Count) < double.Epsilon,
                        "!=" or "<>" => Math.Abs(rightNumber - leftValues.Count) > double.Epsilon,
                        ">" => rightNumber < leftValues.Count,
                        "<" => rightNumber > leftValues.Count,
                        ">=" => rightNumber <= leftValues.Count,
                        "<=" => rightNumber >= leftValues.Count,
                        _ => false
                    };
                }

                return op switch
                {
                    "~=" or "=~" => leftValues.Contains(right),
                    "!~" or "~!" => !leftValues.Contains(right),
                    _ => false
                };
            }
            if (right is IList rv)
            {
                var rightValues = new List<string>();
                foreach (var item in rv)
                {
                    rightValues.Add(item.ToString());
                }
                
                if(left.TryParseNumber(out double leftNumber))
                {
                    return op switch
                    {
                        "~=" or "=~" => rightValues.Any(item => item == left.ToString()),
                        "!~" or "~!" => rightValues.All(item => item != left.ToString()),
                        "=" or "==" => Math.Abs(leftNumber - rightValues.Count) < double.Epsilon,
                        "!=" or "<>" => Math.Abs(leftNumber - rightValues.Count) > double.Epsilon,
                        ">" => leftNumber > rightValues.Count,
                        "<" => leftNumber < rightValues.Count,
                        ">=" => leftNumber >= rightValues.Count,
                        "<=" => leftNumber <= rightValues.Count,
                        _ => false
                    };
                }
                
                return op switch
                {
                    "~=" or "=~" => rightValues.Contains(left),
                    "!~" or "~!" => !rightValues.Contains(left),
                    _ => false
                };
            }

            // Numerical comparison
            if (left.TryParseNumber(out double leftNum) && right.TryParseNumber(out double rightNum))
            {
                return op switch
                {
                    "=" or "==" => Math.Abs(leftNum - rightNum) < double.Epsilon,
                    "!=" or "<>" => Math.Abs(leftNum - rightNum) > double.Epsilon,
                    "~=" or "=~" => leftNum.ToString(CultureInfo.InvariantCulture).Contains(rightNum.ToString(CultureInfo.InvariantCulture)),
                    "!~" or "~!" => !leftNum.ToString(CultureInfo.InvariantCulture).Contains(rightNum.ToString(CultureInfo.InvariantCulture)),
                    ">" => leftNum > rightNum,
                    "<" => leftNum < rightNum,
                    ">=" => leftNum >= rightNum,
                    "<=" => leftNum <= rightNum,
                    _ => false
                };
            }
            
            // Handle enum comparisons
            if (left is Enum leftEnum && right is string rightString)
            {
                try
                {
                    var rightEnum = Enum.Parse(leftEnum.GetType(), rightString, true);
                    return leftEnum.Equals(rightEnum);
                }
                catch
                {
                    // Fall back to string comparison
                }
            }

            // String comparison
            string leftStr = left.ToString();
            string rightStr = right.ToString();

            return op switch
            {
                "=" or "==" => leftStr.Equals(rightStr, StringComparison.OrdinalIgnoreCase),
                "!=" or "<>" => !leftStr.Equals(rightStr, StringComparison.OrdinalIgnoreCase),
                "~=" or "=~" => leftStr.Contains(rightStr),
                "!~" or "~!" => !leftStr.Contains(rightStr),
                ">" => string.Compare(leftStr, rightStr, StringComparison.OrdinalIgnoreCase) > 0,
                "<" => string.Compare(leftStr, rightStr, StringComparison.OrdinalIgnoreCase) < 0,
                ">=" => string.Compare(leftStr, rightStr, StringComparison.OrdinalIgnoreCase) >= 0,
                "<=" => string.Compare(leftStr, rightStr, StringComparison.OrdinalIgnoreCase) <= 0,
                _ => false
            };
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