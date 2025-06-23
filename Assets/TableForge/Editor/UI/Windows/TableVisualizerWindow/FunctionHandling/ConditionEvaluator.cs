using System;
using System.Collections.Generic;

namespace TableForge.Editor.UI
{
    internal static class ConditionEvaluator
    {
        public static Func<object, bool> Evaluate(string op, object rightValue)
        {
            if (string.IsNullOrEmpty(op))
                return _ => false;

            return leftValue => Evaluate(leftValue, op, rightValue);
        }
        
        public static bool Evaluate(object leftValue, string op, object rightValue)
        {
            if (string.IsNullOrEmpty(op))
                return false;
            
            if(leftValue is List<Cell> leftCellList) leftValue = leftCellList.Count >= 1 ? leftCellList[0].GetValue() : null;
            if(rightValue is List<Cell> rightCellList) rightValue = rightCellList.Count >= 1 ? rightCellList[0].GetValue() : null;

            if(TryBooleanComparison(leftValue, op, rightValue, out bool booleanResult))
                return booleanResult;
            
            // Handle numeric comparisons
            if (TryNumericComparison(leftValue, op, rightValue, out bool numericResult))
                return numericResult;

            // Handle string comparisons
            if (TryStringComparison(leftValue, op, rightValue, out bool stringResult))
                return stringResult;

            return false;
        }

        private static bool TryNumericComparison(object left, string op, object right, out bool result)
        {
            result = false;

            if (left == null || right == null)
                return false;
            
            if(!left.TryParseNumber(out double leftValue) || 
               !right.TryParseNumber(out double rightValue))
            {
                return false; // Not a valid numeric comparison
            }
           
            // Perform numeric comparison
            result = op switch
            {
                "=" => Math.Abs(leftValue - rightValue) < double.Epsilon,
                "<>" => Math.Abs(leftValue - rightValue) > double.Epsilon,
                "!=" => Math.Abs(leftValue - rightValue) > double.Epsilon,
                ">" => leftValue > rightValue,
                "<" => leftValue < rightValue,
                ">=" => leftValue >= rightValue,
                "<=" => leftValue <= rightValue,
                _ => false
            };

            return true;
        }

        private static bool TryStringComparison(object left, string op, object right, out bool result)
        {
            result = false;
            if (left == null || right == null)
                return false;
            
            string stringValue = ConvertToString(left);
            string comparisonValue = ConvertToString(right);
            if (string.IsNullOrEmpty(stringValue) || string.IsNullOrEmpty(comparisonValue))
                return false; // Not a valid string comparison
            
            // Perform string comparison
            result = op switch
            {
                "=" => string.Equals(stringValue, comparisonValue, StringComparison.OrdinalIgnoreCase),
                "<>" => !string.Equals(stringValue, comparisonValue, StringComparison.OrdinalIgnoreCase),
                _ => false
            };

            return true;
        }
        
        private static bool TryBooleanComparison(object left, string op, object right, out bool result)
        {
            result = false;
            if (left == null || right == null)
                return false;
            
            if(!left.TryParseBoolean(out bool leftBool) || 
               !right.TryParseBoolean(out bool rightBool))
            {
                return false; // Not a valid boolean comparison
            }

            // Perform boolean comparison
            result = op switch
            {
                "=" => leftBool == rightBool,
                "<>" => leftBool != rightBool,
                "!=" => leftBool != rightBool,
                _ => false
            };
            return true;
        }

        private static string ConvertToString(object value)
        {
            return value?.ToString() ?? string.Empty;
        }
    }
}