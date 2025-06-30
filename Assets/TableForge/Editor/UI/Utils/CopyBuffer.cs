using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TableForge.Editor.UI
{
    internal static class CopyBuffer
    {
        private static Dictionary<TableMetadata, Cell> _lastFunctionsCopiedFrom = new();
        
        #region Public Methods

        public static (Cell firstCell, Cell lastCell) Paste(List<Cell> pasteTo, TableControl tableControl, bool pasteFunctions)
        {
            if (pasteTo == null || pasteTo.Count == 0) return (null, null);
            string buffer = ClipboardUtility.PasteFromClipboard();

            ConfinedSpaceNavigator confinedNavigator = new ConfinedSpaceNavigator(pasteTo, tableControl.Metadata, null);
            List<Cell> cells = FilterCells(confinedNavigator.Cells);
            FreeSpaceNavigator navigator = new FreeSpaceNavigator(tableControl.Metadata, cells[0]);

            int cellCount = 0;
            foreach (var cell in cells)
            {
                if (cell is SubTableCell subTableCell and not ICollectionCell)
                {
                    cellCount += subTableCell.GetDescendantCount(true, false);
                }
                else cellCount++;
            }

            List<string> splitBuffer = new List<string>();
            List<List<string>> rowSplitBuffer = new List<List<string>>();
            string[] rows = buffer.Split(SerializationConstants.RowSeparator);
            foreach (string row in rows)
            {
                string[] splitRow = row.Split(SerializationConstants.ColumnSeparator);
                splitBuffer.AddRange(splitRow);

                List<string> rowBuffer = new List<string>();
                rowBuffer.AddRange(splitRow);
                rowSplitBuffer.Add(rowBuffer);
            }

            if (cellCount >= splitBuffer.Count || splitBuffer.Count > 1000)
                Paste(cells, splitBuffer, tableControl, pasteFunctions);
            else
            {
                Paste(navigator, rowSplitBuffer, tableControl, pasteFunctions);
                return (cells[0], navigator.GetCurrentCell());
            }
            
            return (cells[0], cells[^1]);
        }

        public static void Copy(List<Cell> cellsToCopy, TableMetadata tableMetadata, bool copyFunction)
        {
            if (cellsToCopy == null || cellsToCopy.Count == 0) return;
            
            ConfinedSpaceNavigator navigator = new ConfinedSpaceNavigator(cellsToCopy, tableMetadata, null);
            List<Cell> cells = FilterCells(navigator.Cells);

            bool lastCopiedFunctionSet = false;
            if (!copyFunction) _lastFunctionsCopiedFrom.Remove(tableMetadata);
            
            StringBuilder buffer = new();
            for (int i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                if (i > 0)
                {
                    Table commonTable = cell.GetNearestCommonTable(cells[i - 1], out var cell1Ancestor, out var cell2Ancestor);
                    bool isTransposed = !commonTable.IsSubTable && tableMetadata.IsTransposed;
                    bool isSameRow = isTransposed
                        ? cell1Ancestor.column == cell2Ancestor.column
                        : cell1Ancestor.row == cell2Ancestor.row;

                    if (!isSameRow)
                    {
                        if (buffer.ToString().EndsWith(SerializationConstants.ColumnSeparator))
                            buffer.Length -= SerializationConstants.ColumnSeparator.Length;
                        buffer.Append(SerializationConstants.RowSeparator);
                    }
                }
                
                string formattedValue = copyFunction ? tableMetadata.GetFunction(cell.Id) : cell.Serialize();
                if (cell is StringCell && !copyFunction)
                {
                    formattedValue = formattedValue
                        .Replace(SerializationConstants.RowSeparator, SerializationConstants.CancelledRowSeparator)
                        .Replace(SerializationConstants.ColumnSeparator, SerializationConstants.CancelledColumnSeparator);
                }

                if (copyFunction && !lastCopiedFunctionSet)
                {
                    _lastFunctionsCopiedFrom[tableMetadata] = cell;
                    lastCopiedFunctionSet = true;
                }
                
                buffer.Append(formattedValue);
                if (i != cells.Count - 1) buffer.Append(SerializationConstants.ColumnSeparator);
            }

            ClipboardUtility.CopyToClipboard(buffer.ToString());
        }

        #endregion

        #region Private Methods

        private static void Paste(IList<Cell> cells,  List<string> buffer, TableControl tableControl, bool pasteFunctions)
        {
            int bufferIndex = 0;
            CommandCollection commandCollection = new CommandCollection();

            foreach (var cell in cells)
            {
                string data = buffer[bufferIndex];
            
                if (cell is SubTableCell subTableCell and not ICollectionCell && !JsonUtil.IsValidJsonObject(data))
                {
                    int count = 0;

                    if (!pasteFunctions)
                    {
                        //Get the corresponding cells for this subTable
                        StringBuilder serializedData = new StringBuilder();
                        for (int i = bufferIndex; i < buffer.Count && i < bufferIndex + subTableCell.GetDescendantCount(true, false); i++)
                        {
                            serializedData.Append(buffer[i]).Append(SerializationConstants.ColumnSeparator);
                            count++;
                        }

                        // Remove the last column separator
                        if (serializedData.Length > 0)
                        {
                            serializedData.Remove(serializedData.Length - SerializationConstants.ColumnSeparator.Length,
                                SerializationConstants.ColumnSeparator.Length);
                        }

                        object oldValue = subTableCell.GetValue();
                        if (cell.TryDeserialize(serializedData.ToString()))
                        {
                            SetCellValueCommand command = new SetCellValueCommand(subTableCell, tableControl, oldValue, subTableCell.GetValue());
                            commandCollection.AddAndExecuteCommand(command);
                        }
                    }
                    else
                    {
                        FreeSpaceNavigator subTableNavigator = new FreeSpaceNavigator(tableControl.Metadata, subTableCell);
                        _lastFunctionsCopiedFrom.TryGetValue(tableControl.Metadata, out Cell offsetFromCell);
                        for (int i = bufferIndex; i < buffer.Count && i < bufferIndex + subTableCell.GetDescendantCount(true, false); i++)
                        {
                            Cell subCell = subTableNavigator.GetNextCell(1);
                            while (subCell is SubTableCell && subCell != subTableCell)
                            {
                                subCell = subTableNavigator.GetNextCell(1);
                            }

                            string function = buffer[i];
                            if (offsetFromCell != null)
                            {
                                function = FunctionParser.OffsetFunction(function, offsetFromCell.GetGlobalPosition(), cell.GetGlobalPosition(), cell.GetHighestAncestor().Table);
                            }
                            else offsetFromCell = subCell;
                            
                            IUndoableCommand command = new SetFunctionCommand(cell.Id, function, tableControl.Metadata.GetFunction(cell.Id), tableControl);
                            commandCollection.AddAndExecuteCommand(command);
                            count++;
                        }
                    }

                    bufferIndex = (bufferIndex + count) % buffer.Count;
                }
                else
                {
                    if (!pasteFunctions)
                    {
                        if (cell is StringCell)
                            data = data
                                .Replace(SerializationConstants.CancelledRowSeparator,
                                    SerializationConstants.RowSeparator)
                                .Replace(SerializationConstants.CancelledColumnSeparator,
                                    SerializationConstants.ColumnSeparator);

                        object oldValue = cell.GetValue();
                        if (cell.TryDeserialize(data))
                        {   
                            SetCellValueCommand command = new SetCellValueCommand(cell, tableControl, oldValue, cell.GetValue());
                            commandCollection.AddAndExecuteCommand(command);
                        }
                        
                        if (cell is not SubTableCell) //Recalculate the cell size for the new value
                        {
                            CellControlFactory.GetCellControlFromId(cell.Id)?.RecalculateSize();
                        }
                    }
                    else
                    {
                        _lastFunctionsCopiedFrom.TryGetValue(tableControl.Metadata, out Cell offsetFromCell);
                        string function = data;
                        if (offsetFromCell != null)
                        {
                            function = FunctionParser.OffsetFunction(function, offsetFromCell.GetGlobalPosition(), cell.GetGlobalPosition(), cell.GetHighestAncestor().Table);
                        }

                        IUndoableCommand command = new SetFunctionCommand(cell.Id, function, tableControl.Metadata.GetFunction(cell.Id), tableControl);
                        commandCollection.AddAndExecuteCommand(command);
                    }

                    bufferIndex = (bufferIndex + 1) % buffer.Count;
                }
            }
            
            UndoRedoManager.AddToQueue(commandCollection);
        }

        private static void Paste(FreeSpaceNavigator navigator, List<List<string>> buffer, TableControl tableControl, bool pasteFunctions)
        {
            int bufferIndex = 0;
            int cellIndex = 0;
            Cell currentCell = navigator.GetCurrentCell();
            CommandCollection commandCollection = new CommandCollection();
            
            while (bufferIndex < buffer.Count)
            {
                if (currentCell == null) break;
                string data = buffer[bufferIndex][cellIndex];

                if(currentCell is SubTableCell subTableCell and not ICollectionCell && data != SerializationConstants.EmptyColumn && !JsonUtil.IsValidJsonObject(data))
                {
                    int count = 0;

                    if (!pasteFunctions)
                    {
                        //Get the corresponding cells for this subTable
                        StringBuilder serializedData = new StringBuilder();
                        for (int i = cellIndex; i < buffer[bufferIndex].Count && i < cellIndex + subTableCell.GetDescendantCount(true, false); i++)
                        {
                            serializedData.Append(buffer[bufferIndex][i]).Append(SerializationConstants.ColumnSeparator);
                            count++;
                        }
                            
                        // Remove the last column separator
                        if (serializedData.Length > 0)
                        {
                            serializedData.Remove(serializedData.Length - SerializationConstants.ColumnSeparator.Length, SerializationConstants.ColumnSeparator.Length);
                        }
                        
                        object oldValue = subTableCell.GetValue();
                        if (subTableCell.TryDeserialize(serializedData.ToString()))
                        {
                            SetCellValueCommand command = new SetCellValueCommand(subTableCell, tableControl, oldValue, subTableCell.GetValue());
                            commandCollection.AddAndExecuteCommand(command);
                        }
                    }
                    else 
                    {
                        FreeSpaceNavigator subTableNavigator = new FreeSpaceNavigator(tableControl.Metadata, subTableCell);
                        _lastFunctionsCopiedFrom.TryGetValue(tableControl.Metadata, out Cell offsetFromCell);
                        for (int i = cellIndex; i < buffer[bufferIndex].Count && i < cellIndex + subTableCell.GetDescendantCount(true, false); i++)
                        {
                            Cell cell = subTableNavigator.GetNextCell(1);
                            while (cell is SubTableCell && cell != subTableCell)
                            {
                                cell = subTableNavigator.GetNextCell(1);
                            }

                            string function = buffer[bufferIndex][i];
                            if (offsetFromCell != null)
                            {
                                function = FunctionParser.OffsetFunction(function, offsetFromCell.GetGlobalPosition(), cell.GetGlobalPosition(), cell.GetHighestAncestor().Table);
                            }
                            else offsetFromCell = cell;

                            IUndoableCommand command = new SetFunctionCommand(cell.Id, function, tableControl.Metadata.GetFunction(cell.Id), tableControl);
                            commandCollection.AddAndExecuteCommand(command);
                            count++;
                        }
                    }
                    
                    bool subTableWasEmpty = subTableCell.SubTable.Rows.Count == 0;
                    if(subTableWasEmpty) navigator.GetNextCell(0);
                    for (int i = 0; i < count - 1; i++)
                    {
                        navigator.GetNextCell(1);
                    }

                    cellIndex += count;
                    while (cellIndex > buffer[bufferIndex].Count)
                    {
                        cellIndex -= buffer[bufferIndex].Count;
                        bufferIndex++;
                        if (bufferIndex >= buffer.Count) break;
                    }
                }
                else if (data != SerializationConstants.EmptyColumn)
                {
                    if (!pasteFunctions)
                    {
                        if (currentCell is StringCell)
                            data = data
                                .Replace(SerializationConstants.CancelledRowSeparator,
                                    SerializationConstants.RowSeparator)
                                .Replace(SerializationConstants.CancelledColumnSeparator,
                                    SerializationConstants.ColumnSeparator);

                        
                        object oldValue = currentCell.GetValue();
                        if (currentCell.TryDeserialize(data))
                        {
                            SetCellValueCommand command = new SetCellValueCommand(currentCell, tableControl, oldValue, currentCell.GetValue());
                            commandCollection.AddAndExecuteCommand(command);
                        }
                        
                        if (currentCell is not SubTableCell) //Recalculate the cell size for the new value
                        {
                            CellControlFactory.GetCellControlFromId(currentCell.Id)?.RecalculateSize();
                        }
                    }
                    else
                    {
                        _lastFunctionsCopiedFrom.TryGetValue(tableControl.Metadata, out Cell offsetFromCell);
                        string function = data;
                        if (offsetFromCell != null)
                        {
                            function = FunctionParser.OffsetFunction(function, offsetFromCell.GetGlobalPosition(), currentCell.GetGlobalPosition(), currentCell.GetHighestAncestor().Table);
                        }
                        
                        IUndoableCommand command = new SetFunctionCommand(currentCell.Id, function, tableControl.Metadata.GetFunction(currentCell.Id), tableControl);
                        commandCollection.AddAndExecuteCommand(command);
                    }

                    cellIndex++;
                }
                
                if(cellIndex >= buffer[bufferIndex].Count)
                {
                    cellIndex = 0;
                    bufferIndex++;
                    if (bufferIndex >= buffer.Count) break;
                    
                    currentCell = navigator.GetCellAtNextRow(1);
                }
                else currentCell = navigator.GetNextCell(1);
            }
            
            UndoRedoManager.AddToQueue(commandCollection);
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
