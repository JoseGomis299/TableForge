using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TableForge.UI
{
    internal static class CopyBuffer
    {
        #region Fields

        private static readonly CellNavigator _cellNavigator = new();
        
       
        #endregion

        #region Public Methods

        public static void Paste(List<Cell> pasteTo, TableMetadata tableMetadata)
        {
            if (pasteTo == null || pasteTo.Count == 0) return;
            string buffer = ClipboardUtility.PasteFromClipboard();

            _cellNavigator.SetNavigationSpace(pasteTo, tableMetadata, null);
            List<Cell> cells = FilterCells(_cellNavigator.Cells);

            List<string> splitBuffer = new List<string>();
            string[] rows = buffer.Split(SerializationConstants.RowSeparator);
            foreach (string row in rows)
            {
                splitBuffer.AddRange(row.Split(SerializationConstants.ColumnSeparator));
            }
            
            Paste(cells, splitBuffer, 0);
        }

        public static void Copy(List<Cell> cellsToCopy, TableMetadata tableMetadata)
        {
            if (cellsToCopy == null || cellsToCopy.Count == 0) return;
            _cellNavigator.SetNavigationSpace(cellsToCopy, tableMetadata, null);
            List<Cell> cells = FilterCells(_cellNavigator.Cells);
            
            StringBuilder buffer = new();
            for (int i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                if (i > 0 && cell.GetNearestCommonRow(cells[i - 1]) != null)
                {
                    if (buffer.ToString().EndsWith(SerializationConstants.ColumnSeparator))
                        buffer.Length -= SerializationConstants.ColumnSeparator.Length;
                    buffer.Append(SerializationConstants.RowSeparator);
                }

                string formattedValue = cell.Serialize();
                if (cell is StringCell)
                {
                    formattedValue = formattedValue
                        .Replace(SerializationConstants.RowSeparator, SerializationConstants.CancelledRowSeparator)
                        .Replace(SerializationConstants.ColumnSeparator, SerializationConstants.CancelledColumnSeparator);
                }
                buffer.Append(formattedValue);
                if (i != cells.Count - 1) buffer.Append(SerializationConstants.ColumnSeparator);
            }

            ClipboardUtility.CopyToClipboard(buffer.ToString());
        }

        #endregion

        #region Private Methods

        private static void Paste(IList<Cell> cells, IList<string> buffer, int bufferIndex)
        {
            foreach (var cell in cells)
            {
                string data = buffer[bufferIndex];
            
                if (cell is SubTableCell subTableCell and not ICollectionCell)
                {
                    //Get the corresponding cells for this subTable
                    StringBuilder serializedData = new StringBuilder();
                    int count = 0;
                    for (int i = bufferIndex; i < buffer.Count && i < bufferIndex + subTableCell.GetDescendantCount(true, false); i++)
                    {
                        serializedData.Append(buffer[i]).Append(SerializationConstants.ColumnSeparator);
                        count++;
                    }
                    
                    // Remove the last column separator
                    if (serializedData.Length > 0)
                    {
                        serializedData.Remove(serializedData.Length - SerializationConstants.ColumnSeparator.Length, SerializationConstants.ColumnSeparator.Length);
                    }
                    
                    subTableCell.TryDeserialize(serializedData.ToString());
                    bufferIndex += count;
                }
                else
                {
                    if (string.IsNullOrEmpty(data)) continue;
                    if (cell is StringCell)
                        data = data
                            .Replace(SerializationConstants.CancelledRowSeparator, SerializationConstants.RowSeparator)
                            .Replace(SerializationConstants.CancelledColumnSeparator, SerializationConstants.ColumnSeparator);
                    
                    cell.TryDeserialize(data);
                    bufferIndex = (bufferIndex + 1) % buffer.Count;
                }
            }
        }

        private static List<Cell> FilterCells(IEnumerable<Cell> cells)
        {
            List<Cell> filteredCells = new();
            foreach (var cell in cells)
            {
                Cell parent = cell.Table.ParentCell;
                if (filteredCells.Any() && filteredCells[^1] == parent)
                    filteredCells.RemoveAt(filteredCells.Count - 1);

                filteredCells.Add(cell);
            }
            return filteredCells;
        }

        #endregion
    }
}
