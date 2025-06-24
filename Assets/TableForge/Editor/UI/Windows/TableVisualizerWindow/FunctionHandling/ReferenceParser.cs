using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TableForge.Editor.UI
{
    internal class ReferenceParser
    {
        public bool IsReference(string input)
        {
            input = input.Replace("$", ""); // Remove absolute markers
            input = input.Replace(".", ""); // Remove dots
            input = input.Replace(":", ""); // Remove range markers
            input = input.Trim();
            
            Regex regex = new Regex(@"^([A-Z]+[0-9]+)+$");
            return regex.IsMatch(input);
        }
        
        public List<Cell> ResolveReference(string reference, Cell contextCell)
        {
            // Remove absolute markers ($A$1 becomes A1)
            reference = reference.Replace("$", "");
            
            if (reference.Contains(':'))
                return ResolveRange(reference, contextCell);
            
            return new List<Cell> { ResolveSingleCell(reference, contextCell) };
        }

        private Cell ResolveSingleCell(string position, Cell contextCell)
        {
            Table currentTable = contextCell.Table;
            
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

        private List<Cell> ResolveRange(string range, Cell contextCell)
        {
            string[] positions = range.Split(':');
            if (positions.Length != 2)
                throw new FormatException($"Invalid range format: {range}");

            Cell startCell = ResolveSingleCell(positions[0], contextCell);
            Cell endCell = ResolveSingleCell(positions[1], contextCell);
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