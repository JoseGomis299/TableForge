using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace TableForge.Editor.UI
{
    internal static class ReferenceParser
    {
        private const string ReferencePattern =
            @"(\$?[A-Z]+\$?[0-9]+(?:\.\$?[A-Z]+\$?[0-9]+)*(?::\$?[A-Z]+\$?[0-9]+(?:\.\$?[A-Z]+\$?[0-9]+)*)?" //A1, $A$1, A1.B2, $A$1.$B$2, A1:B2, $A$1:$B$2, A1.B2:C3, $A$1.$B$2:$C$3
            + @"|(?:\$?[A-Z]+(?:\.\$?[A-Z]+)*:\$?[A-Z]+(?:\.\$?[A-Z]+)*)"  //A:B, $A:$B, A.B:C, $A.$B:$C
            + @"|(?:\$?[0-9]+(?:\.\$?[0-9]+)*:\$?[0-9]+(?:\.\$?[0-9]+)*))"; //1:2, $1:$2, 1.2:3.4, $1.$2:$3.$4
        
        private const string ColumnPattern = @"^\$?[A-Z]+(?:\.\$?[A-Z]+)*$"; //A, $A, A.B, $A.$B
        private const string RowPattern = @"^\$?[0-9]+(?:\.\$?[0-9]+)*$"; //1, $1, 1.2, $1.$2
        
        public static bool IsReference(string input)
        {
            input = input.Trim();
            
            Regex regex = new Regex("^"+ReferencePattern+"$");
            return regex.IsMatch(input);
        }
        
        public static List<string> ExtractReferences(string input)
        {
            List<string> references = new List<string>();
            MatchCollection matches = Regex.Matches(input, ReferencePattern);
            
            foreach (Match match in matches)
            {
                if (match.Success && FunctionRegistry.GetFunction(match.Value) == null)
                {
                    references.Add(match.Value);
                }
            }
            
            return references;
        }
        
        public static List<Cell> ResolveReference(string reference, Table baseTable)
        {
            if (reference.Contains(':'))
                return ResolveRange(reference, baseTable);
            
            return new List<Cell> { ResolveSingleCell(reference, baseTable) };
        }

        public static string GetRelativeReference(string reference, string originalPosition, string finalPosition, Table baseTable, bool singlePartReferencesStart = false)
        {
            Cell originalCell = ResolveSingleCell(originalPosition, baseTable);
            Cell finalCell = ResolveSingleCell(finalPosition, baseTable);
            
            if (originalCell == null || finalCell == null)
                throw new KeyNotFoundException($"Could not resolve cells for positions: {originalPosition}, {finalPosition}");

            if (finalPosition == originalPosition)
                return reference;
            
            if (reference.Contains(":"))
            {
                List<string> parts = reference.Split(':').ToList();
                if (parts.Count != 2)
                    throw new FormatException($"Invalid range format: {reference}");
                
                return GetRelativeReference(parts[0], originalPosition, finalPosition, baseTable, true) + ":" +
                       GetRelativeReference(parts[1], originalPosition, finalPosition, baseTable, false);
            }

            List<Vector2Int> offsets = finalCell.GetDistancesByDepth(originalCell);
            offsets.Reverse(); //Reverse to match the order of nested references
            
            Regex regex = new Regex("(\\$?[A-Z]+)?(\\$?[0-9]+)?");
            List<string> nestedParts = reference.Split('.').ToList();
            for (int i = nestedParts.Count - 1; i >= 0; i--)
            {
                // Split into column and row parts
                var match = regex.Match(nestedParts[i]);
                string columnPart = match.Groups[1].Value;
                string rowPart = match.Groups[2].Value;

                // Calculate the new column position
                string columnReference = columnPart;
                if (!string.IsNullOrEmpty(columnPart))
                {
                    bool isAbsoluteColumn = columnPart.StartsWith("$");

                    int newColumnPosition = PositionUtil.ConvertToNumber(columnPart.Replace("$", ""));
                    if (!isAbsoluteColumn && i < offsets.Count)
                        newColumnPosition += offsets[i].x;
                    
                    columnReference = isAbsoluteColumn
                        ? $"${PositionUtil.ConvertToLetters(newColumnPosition)}"
                        : PositionUtil.ConvertToLetters(newColumnPosition);
                }
                
                //Calculate the new row position
                string rowReference = rowPart;
                if (!string.IsNullOrEmpty(rowPart))
                {
                    bool isAbsoluteRow = rowPart.StartsWith("$");
                    int newRowPosition = int.Parse(rowPart.Replace("$", ""));     
                    
                    if (!isAbsoluteRow && i < offsets.Count)
                        newRowPosition += offsets[i].y;
                    
                    rowReference = isAbsoluteRow
                        ? $"${newRowPosition}"
                        : newRowPosition.ToString();
                }
                
                nestedParts[i] = $"{columnReference}{rowReference}";
            }
            
            // Join the parts back together
            string result = string.Join(".", nestedParts);
            if(ResolveSingleCell(result, baseTable, singlePartReferencesStart) == null)
            {
                throw new KeyNotFoundException($"Could not resolve relative reference: {result}");
            }
            
            return result;
        }

        private static Cell ResolveSingleCell(string position, Table baseTable, bool singlePartReferencesStart = false)
        {
            Table currentTable = baseTable;
            position = position.Replace("$", ""); // Remove absolute markers
            Regex columnRegex = new Regex(ColumnPattern);
            Regex rowRegex = new Regex(RowPattern);
            
            // Handle nested references (A1.B2)
            if (position.Contains('.'))
            {
                string[] parts = position.Split('.');
                foreach (string part in parts)
                {
                    if (currentTable == null) break;
                    string value = part.Trim();
                    
                    if (columnRegex.IsMatch(part))
                    {
                        value += singlePartReferencesStart ? "1" : currentTable.Rows.Count.ToString();
                    }
                    else if (rowRegex.IsMatch(part))
                    {
                        value = singlePartReferencesStart ? "A" + value : PositionUtil.ConvertToLetters(currentTable.Columns.Count) + value;
                    }
                    
                    Cell cell = currentTable.GetCell(value);
                    if (cell is SubTableCell subTableCell)
                        currentTable = subTableCell.SubTable;
                    else
                        return cell;
                }
                return null;
            }

            if (columnRegex.IsMatch(position))
            {
                position += singlePartReferencesStart ? "1" : currentTable.Rows.Count.ToString();
            }
            else if (rowRegex.IsMatch(position))
            {
                position = singlePartReferencesStart ? "A" + position : PositionUtil.ConvertToLetters(currentTable.Columns.Count) + position;
            }

            return currentTable.GetCell(position);
        }

        private static List<Cell> ResolveRange(string range, Table baseTable)
        {
            range = range.Replace("$", ""); // Remove absolute markers
            string[] positions = range.Split(':');
            if (positions.Length != 2)
                throw new FormatException($"Invalid range format: {range}");
            
            Cell startCell = ResolveSingleCell(positions[0], baseTable, true);
            Cell endCell = ResolveSingleCell(positions[1], baseTable, false);
            
            if (startCell == null || endCell == null)
                throw new KeyNotFoundException($"Could not resolve range: {range}");
            
            int depth = startCell.GetDepth();
            if(depth != endCell.GetDepth())
                throw new InvalidOperationException("Range spans multiple depths");

            bool goesRight = startCell.column.Position <= endCell.column.Position;
            bool goesDown = startCell.row.Position <= endCell.row.Position;
            
            return CellLocator.GetCellRange(startCell, endCell, null).
                Where(c => c.GetDepth() == depth). // Ensure cells are at the same depth
                Where(c => goesRight ? c.column.Position >= startCell.column.Position && c.column.Position <= endCell.column.Position :
                                       c.column.Position <= startCell.column.Position && c.column.Position >= endCell.column.Position).
                Where(c => goesDown ? c.row.Position >= startCell.row.Position && c.row.Position <= endCell.row.Position :
                                       c.row.Position <= startCell.row.Position && c.row.Position >= endCell.row.Position).
                ToList();
        }
    }
}