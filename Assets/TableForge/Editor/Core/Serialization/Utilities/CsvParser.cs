using System.Collections.Generic;
using System.Text;

namespace TableForge.Editor.Serialization
{
    public static class CsvParser
    {
        public static List<List<string>> ParseCsv(string csv)
        {
            var result = new List<List<string>>();
            var currentRow = new List<string>();
            var currentField = new StringBuilder();

            bool insideQuotes = false;
            int i = 0;

            while (i < csv.Length)
            {
                char c = csv[i];

                if (c == '"')
                {
                    if (insideQuotes)
                    {
                        if (i + 1 < csv.Length && csv[i + 1] == '"')
                        {
                            // Convent double quotes to a single quote
                            currentField.Append('"');
                            i++;
                        }
                        else
                        {
                            // End of quoted field
                            insideQuotes = false;
                        }
                    }
                    else
                    {
                        // Start of quoted field
                        insideQuotes = true;
                    }
                }
                else if (c == ',' && !insideQuotes)
                {
                    // End of field
                    currentRow.Add(currentField.ToString());
                    currentField.Clear();
                }
                else if ((c == '\n' || c == '\r') && !insideQuotes)
                {
                    // End of row
                    if (c == '\r' && i + 1 < csv.Length && csv[i + 1] == '\n')
                    {
                        i++; // Skip \n after \r
                    }

                    currentRow.Add(currentField.ToString());
                    currentField.Clear();

                    result.Add(currentRow);
                    currentRow = new List<string>();
                }
                else
                {
                    currentField.Append(c);
                }

                i++;
            }

            // Add last field and row if any
            currentRow.Add(currentField.ToString());
            if (currentRow.Count > 1 || currentRow[0] != "")
            {
                result.Add(currentRow);
            }

            return result;
        }
    }
}