using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace TableForge
{
    internal static class StringExtension
    {
        private static readonly Regex UnderscorePrefixRegex = new(@"^m_|^_", RegexOptions.Compiled);
        private static readonly Regex CamelCaseRegex = new(@"([a-z])([A-Z])", RegexOptions.Compiled);
        private static readonly Regex UnderscoreReplaceRegex = new(@"_+", RegexOptions.Compiled);
        private static readonly Regex LetterNumberRegex = new(@"([a-zA-Z])(\d)", RegexOptions.Compiled);
        private static readonly Regex NumberLetterRegex = new(@"(\d)([A-Z])", RegexOptions.Compiled);

        /// <summary>
        /// Converts a string to proper case by handling common naming conventions.
        /// </summary>
        /// <param name="input">The input string to convert.</param>
        /// <returns>A formatted string in title case.</returns>
        public static string ConvertToProperCase(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Remove common prefixes like "m_" or leading underscores
            input = UnderscorePrefixRegex.Replace(input, "");

            // Add spaces between camel case transitions
            string spaced = CamelCaseRegex.Replace(input, "$1 $2");
            
            //Add spaces between letters and numbers
            spaced = LetterNumberRegex.Replace(spaced, "$1 $2");
            
            //Add spaces between numbers and letters
            spaced = NumberLetterRegex.Replace(spaced, "$1 $2");

            // Replace underscores with spaces and remove excess whitespace
            spaced = UnderscoreReplaceRegex.Replace(spaced, " ").Trim();

            // Convert to title case
            TextInfo textInfo = CultureInfo.InvariantCulture.TextInfo;
            return textInfo.ToTitleCase(spaced.ToLower());
        }
        
        /// <summary>
        /// Splits the input string into segments enclosed by specified open and close strings at a given nesting level.
        /// </summary>
        /// <param name="input">The input string to process.</param>
        /// <param name="desiredLevel">The target closure level (0-based). Level 0 corresponds to the content within the first occurrence of the open string.</param>
        /// <param name="openStr">The string used to denote the start of a closure (e.g., "{").</param>
        /// <param name="closeStr">The string used to denote the end of a closure (e.g., "}").</param>
        /// <returns>A list of strings representing the segments captured at the specified level.</returns>
        public static List<string> SplitByLevel(this string input, int desiredLevel, string openStr, string closeStr)
        {
            List<string> segments = new List<string>();
            StringBuilder currentSegment = new StringBuilder();
            int depth = -1;
            bool capturing = false;
            int i = 0;

            while (i < input.Length)
            {
                if (i <= input.Length - openStr.Length && input.Substring(i, openStr.Length) == openStr)
                {
                    depth++;
                    if (depth == desiredLevel)
                    {
                        currentSegment.Clear();
                        capturing = true;
                    }
                    else if (capturing && depth > desiredLevel)
                    {
                        currentSegment.Append(openStr);
                    }
                    i += openStr.Length;
                }
                else if (i <= input.Length - closeStr.Length && input.Substring(i, closeStr.Length) == closeStr)
                {
                    if (capturing && depth == desiredLevel)
                    {
                        segments.Add(currentSegment.ToString().Trim());
                        currentSegment.Clear();
                        capturing = false;
                    }
                    else if (capturing && depth > desiredLevel)
                    {
                        currentSegment.Append(closeStr);
                    }
                    depth--;
                    i += closeStr.Length;
                }
                else
                {
                    if (capturing)
                        currentSegment.Append(input[i]);
                    i++;
                }
            }

            return segments;
        }

    }
}