using System;

namespace TableForge.Editor
{
    /// <summary>
    /// Provides utility methods for converting and manipulating spreadsheet-style column/row positions.
    /// </summary>
    internal static class PositionUtil
    {
        #region Constants

        private const char MaxColPos = 'Z';
        private const char MinColPos = 'A';
        private const int ColPosCount = 'Z' - 'A' + 1;

        #endregion

        #region Public Methods

        /// <summary>
        /// Converts a numeric column position to its corresponding spreadsheet-style letter representation.
        /// </summary>
        /// <param name="position">1-based numeric column position (e.g., 1 → "A", 27 → "AA").</param>
        /// <returns>Column position represented as letters.</returns>
        /// <exception cref="ArgumentException">Thrown when position is less than or equal to 0.</exception>
        public static string ConvertToLetters(int position)
        {
            if(!IsValidNumericPosition(position)) 
                throw new ArgumentException("Invalid position");
            
            var result = string.Empty;
            while (position > 0)
            {
                var remainder = position % ColPosCount;
                if (remainder == 0)
                {
                    result = MaxColPos + result;
                    position = (position / ColPosCount) - 1;
                }
                else
                {
                    result = (char) (remainder - 1 + MinColPos) + result;
                    position = position / ColPosCount;
                }
            }

            return result;
        }

        /// <summary>
        /// Converts a spreadsheet-style column letter representation to its corresponding numeric position.
        /// </summary>
        /// <param name="position">Column position represented as letters (e.g., "A" → 1, "BC" → 55).</param>
        /// <returns>1-based numeric column position.</returns>
        /// <exception cref="ArgumentException">Thrown when input contains non-alphabetic characters or invalid column letters.</exception>
        public static int ConvertToNumber(string position)
        {
            if (!IsValidLetterPosition(position))
                throw new ArgumentException("Invalid position");

            var result = 0;
            for (var i = 0; i < position.Length; i++)
            {
                result = result * ColPosCount + position[i] - MinColPos + 1;
            }

            return result;
        }
        
        /// <summary>
        /// Combines numeric column and row positions into a spreadsheet-style cell position string.
        /// </summary>
        /// <param name="column">1-based numeric column position.</param>
        /// <param name="row">1-based numeric row position.</param>
        /// <returns>Combined position string in the format "A1".</returns>
        /// <exception cref="ArgumentException">Thrown when column or row is invalid (≤ 0).</exception>
        public static string GetPosition(int column, int row)
        {
            if(!IsValidNumericPosition(column) || !IsValidNumericPosition(row))
                throw new ArgumentException("Invalid position");
            
            return $"{ConvertToLetters(column)}{row}";
        }
        
        /// <summary>
        /// Parses a spreadsheet-style cell position string into numeric column and row components.
        /// </summary>
        /// <param name="position">Position string in the format "A1", "BC23", etc.</param>
        /// <returns>Tuple containing (column, row) as numeric positions.</returns>
        /// <exception cref="ArgumentException">Thrown for invalid format, non-alphabetic column characters, or invalid row number.</exception>
        public static (int column, int row) GetPosition(string position)
        {
            int columnEndIndex = 0;
            while (position[columnEndIndex] is >= MinColPos and <= MaxColPos)
            {
                columnEndIndex++;
            }
            
            string columnSubstring = position.Substring(0, columnEndIndex);
            int column = ConvertToNumber(columnSubstring);
            
            if(!int.TryParse(position.Substring(columnEndIndex), out int row))
                throw new ArgumentException($"Invalid position {position} - row is invalid");
            
            return (column, row);
        }

        #endregion
        
        #region Private Methods

        private static bool IsValidNumericPosition(int position)
        {
            return position > 0;
        }
        
        private static bool IsValidLetterPosition(string position)
        {
            foreach (var character in position)
            {
                if (character is < MinColPos or > MaxColPos)
                    return false;
            }

            return true;
        }
        
        #endregion
    }
}