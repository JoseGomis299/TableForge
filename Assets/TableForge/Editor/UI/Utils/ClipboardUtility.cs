using System.Collections.Generic;
using UnityEngine;

namespace TableForge.Editor.UI
{
    internal static class ClipboardUtility
    {
        public static void CopyToClipboard(List<List<string>> text, char columnSeparator = '\t', char rowSeparator = '\n')
        {
            string formattedText = string.Empty;
            foreach (var row in text)
            {
                formattedText += string.Join(columnSeparator.ToString(), row) + rowSeparator;
            }
            formattedText = formattedText.TrimEnd(rowSeparator);
            GUIUtility.systemCopyBuffer = formattedText;
        }
        
        public static void CopyToClipboard(string text)
        {
            GUIUtility.systemCopyBuffer = text;
        }

        public static string PasteFromClipboard()
        {
            return GUIUtility.systemCopyBuffer;
        }
        
        public static List<List<string>> PasteFromFormattedClipboard(char columnSeparator = '\t', char rowSeparator = '\n')
        {
            string clipboardText = GUIUtility.systemCopyBuffer;
            if (string.IsNullOrEmpty(clipboardText))
                return new List<List<string>>();

            var rows = clipboardText.Split(new[] { rowSeparator }, System.StringSplitOptions.RemoveEmptyEntries);
            var result = new List<List<string>>();

            foreach (var row in rows)
            {
                var columns = row.Split(new[] { columnSeparator }, System.StringSplitOptions.None);
                result.Add(new List<string>(columns));
            }

            return result;
        }
    }
}