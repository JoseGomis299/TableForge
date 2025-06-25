using System.Collections.Generic;

namespace TableForge.Editor.UI
{
    internal static class FunctionArgumentHelper
    {
        public static bool TryGetSingleNumber(object arg, out double number)
        {
            number = 0;
            if (arg == null) return false;

            if (arg.TryParseNumber(out number))
            {
                return true;
            }
            if (arg is List<Cell> cells && cells.Count > 0)
            {
                var val = cells[0].GetValue();
                if (val != null && val.TryParseNumber(out double num))
                {
                    number = num;
                    return true;
                }
            }
            return false;
        }

        public static bool ConvertToBoolean(object arg)
        {
            if (arg is bool b) return b;
        
            if (arg is List<Cell> cells)
            {
                // For cell ranges, consider true if at least one cell is truthy
                foreach (var cell in cells)
                {
                    if (IsCellTruthy(cell)) return true;
                }
                return false;
            }
        
            return false;
        }

        private static bool IsCellTruthy(Cell cell)
        {
            if (cell == null) return false;
        
            var value = cell.GetValue();
            if (value is bool b) return b;
        
            return false;
        }
    }
}