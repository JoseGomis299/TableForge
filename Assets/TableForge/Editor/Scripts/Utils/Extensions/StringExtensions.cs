using System.Globalization;
using System.Text.RegularExpressions;

namespace TableForge
{
    internal static class StringExtensions
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
    }
}