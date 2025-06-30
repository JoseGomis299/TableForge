using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace TableForge.Editor.UI
{
    internal static class ReferenceParser
    {
        public static bool IsReference(string input)
        {
            input = input.Replace("$", ""); // Remove absolute markers
            input = input.Replace(".", ""); // Remove dots
            input = input.Replace(":", ""); // Remove range markers
            input = input.Trim();
            
            Regex regex = new Regex(@"^([A-Z]+[0-9]+)+$");
            return regex.IsMatch(input);
        }
        
        public static List<string> ExtractReferences(string input)
        {
            List<string> references = new List<string>();
            string pattern = @"(\$?[A-Z]+\$?[0-9]+(?:\.\$?[A-Z]+\$?[0-9]+)*(?::\$?[A-Z]+\$?[0-9]+(?:\.\$?[A-Z]+\$?[0-9]+)*)?)";
            MatchCollection matches = Regex.Matches(input, pattern);
            
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

        public static string GetRelativeReference(string reference, string originalPosition, string finalPosition, Table baseTable)
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
                
                return GetRelativeReference(parts[0], originalPosition, finalPosition, baseTable) + ":" +
                       GetRelativeReference(parts[1], originalPosition, finalPosition, baseTable);
            }

            List<Vector2Int> offsets = finalCell.GetDistancesByDepth(originalCell);
            int offsetIndex = offsets.Count - 1;
            
            Regex regex = new Regex("(\\$?[A-Z]+)(\\$?[0-9]+)");
            List<string> nestedParts = reference.Split('.').ToList();
            for (int i = nestedParts.Count - 1; i >= 0; i--)
            {
                var match = regex.Match(nestedParts[i]);
                
                // Split into column and row parts
                string columnPart = match.Groups[1].Value;
                string rowPart = match.Groups[2].Value;
                
                if (string.IsNullOrEmpty(columnPart) || string.IsNullOrEmpty(rowPart))
                    throw new FormatException($"Invalid reference part: {nestedParts[i]}");

                // Calculate new positions based on offsets
                int newColumnPosition = PositionUtil.ConvertToNumber(columnPart.Replace("$", ""));
                int newRowPosition = int.Parse(rowPart.Replace("$", ""));     
                
                bool isAbsoluteColumn = columnPart.StartsWith("$");
                bool isAbsoluteRow = rowPart.StartsWith("$");
                
                if (!isAbsoluteColumn && offsetIndex >= 0)
                    newColumnPosition += offsets[offsetIndex].x;

                if (!isAbsoluteRow && offsetIndex >= 0)
                    newRowPosition += offsets[offsetIndex].y;
                

                // Convert back to Excel-style reference
                string columnReference = isAbsoluteColumn
                    ? $"${PositionUtil.ConvertToLetters(newColumnPosition)}"
                    : PositionUtil.ConvertToLetters(newColumnPosition);
                string rowReference = isAbsoluteRow
                    ? $"${newRowPosition}"
                    : newRowPosition.ToString();
                
                nestedParts[i] = $"{columnReference}{rowReference}";
                offsetIndex--;
            }
            
            // Join the parts back together
            string result = string.Join(".", nestedParts);
            if(ResolveSingleCell(result, baseTable) == null)
            {
                throw new KeyNotFoundException($"Could not resolve relative reference: {result}");
            }
            
            return result;
        }

        private static Cell ResolveSingleCell(string position, Table baseTable)
        {
            Table currentTable = baseTable;
            position = position.Replace("$", ""); // Remove absolute markers
            
            // Handle nested references (A1.B2)
            if (position.Contains('.'))
            {
                string[] parts = position.Split('.');
                foreach (string part in parts)
                {
                    if (currentTable == null) break;
                    
                    Cell cell = currentTable.GetCell(part);
                    if (cell is SubTableCell subTableCell)
                        currentTable = subTableCell.SubTable;
                    else
                        return cell;
                }
                return null;
            }
            
            return currentTable.GetCell(position);
        }

        private static List<Cell> ResolveRange(string range, Table baseTable)
        {
            range = range.Replace("$", ""); // Remove absolute markers
            string[] positions = range.Split(':');
            if (positions.Length != 2)
                throw new FormatException($"Invalid range format: {range}");

            Cell startCell = ResolveSingleCell(positions[0], baseTable);
            Cell endCell = ResolveSingleCell(positions[1], baseTable);
            int depth = startCell.GetDepth();
            
            if (startCell == null || endCell == null)
                throw new KeyNotFoundException($"Could not resolve range: {range}");
            
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