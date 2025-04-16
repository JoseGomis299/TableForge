using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace TableForge.UI
{
    internal static class CopyBuffer
    {
        #region Constants

        private const string ColumnSeparator = "\t";
        private const string CancelledColumnSeparator = "\\t";
        private const string RowSeparator = "\n";
        private const string CancelledRowSeparator = "\\n";

        private const string CollectionSeparator = " || ";
        private const string CollectionStart = "\\Array: ";
        private const string CollectionItemStart = "{";
        private const string CollectionItemEnd = "}";
        private const string CollectionItemSeparator = ", ";
        private const string DictionaryKeysStart = "\\Keys: ";
        private const string DictionaryValuesStart = " \\Values: ";

        #endregion

        #region Fields

        private static readonly CellNavigator _cellNavigator = new();

        #endregion

        #region Public Methods

        public static void Paste(ISerializableCell pasteTo)
        {
            if (pasteTo == null) return;
            string buffer = ClipboardUtility.PasteFromClipboard();
            List<string> splitBuffer = SplitBufferIntoPlainList(buffer);
            pasteTo.TryDeserialize(splitBuffer[0]);
        }

        public static void Paste(List<Cell> pasteTo, TableMetadata tableMetadata)
        {
            if (pasteTo == null || pasteTo.Count == 0) return;
            string buffer = ClipboardUtility.PasteFromClipboard();

            _cellNavigator.SetNavigationSpace(pasteTo, tableMetadata, null);
            List<Cell> cells = FilterCells(_cellNavigator.Cells);

            List<string> splitBuffer = SplitBufferIntoPlainList(buffer);
            Paste(cells, splitBuffer, 0);
        }

        public static void Copy(ISerializableCell cellToCopy)
        {
            if (cellToCopy == null) return;
            ClipboardUtility.CopyToClipboard(cellToCopy.Serialize());
        }

        public static void Copy(List<Cell> cellsToCopy, TableMetadata tableMetadata)
        {
            if (cellsToCopy == null || cellsToCopy.Count == 0) return;
            _cellNavigator.SetNavigationSpace(cellsToCopy, tableMetadata, null);
            List<Cell> cells = FilterCells(_cellNavigator.Cells);
            ClipboardUtility.CopyToClipboard(Copy(cells, false));
        }

        #endregion

        #region Private Methods

        private static int Paste(IList<Cell> cells, IList<string> buffer, int bufferIndex)
        {
            int pastedCells = 0;

            foreach (var cell in cells)
            {
                string data = buffer[bufferIndex];

                if (cell is SubTableCell subTableCell)
                {
                    pastedCells += PasteSubTableCell(subTableCell, buffer, ref bufferIndex, data);
                }
                else
                {
                    bufferIndex = (bufferIndex + 1) % buffer.Count;
                    pastedCells++;
                }

                if (string.IsNullOrEmpty(data)) continue;
                data = data.Replace(CancelledRowSeparator, RowSeparator).Replace(CancelledColumnSeparator, ColumnSeparator);
                cell.TryDeserialize(data);
            }

            return pastedCells;
        }

        private static int PasteSubTableCell(SubTableCell subTableCell, IList<string> buffer, ref int bufferIndex, string data)
        {
            if (subTableCell.SubTable.Rows.Any())
            {
                if (subTableCell is ICollectionCell)
                {
                    bool isCollectionValue = data.StartsWith(CollectionStart);
                    bool isDictionaryValue = data.StartsWith(DictionaryKeysStart);
                    List<string> collectionBuffer = new();
                    
                    if (isCollectionValue)
                    {
                        PasteCollection(subTableCell, data, collectionBuffer);
                        bufferIndex = (bufferIndex + 1) % buffer.Count;
                        return 1;
                    }
                    
                    if (isDictionaryValue && subTableCell is DictionaryCell)
                    {
                        PasteDictionary(subTableCell, data, collectionBuffer);
                        bufferIndex = (bufferIndex + 1) % buffer.Count;
                        return 1;
                    }

                    //Reaching this point means that the data to paste is not compatible with the subTable, or it represents an empty collection
                    return 0;
                }

                //If the subTable is not a collection we will paste the values to the immediate descendants of the subTable
                int subTableCells = Paste(subTableCell.GetImmediateDescendants().ToList(), buffer, bufferIndex);
                bufferIndex = (bufferIndex + subTableCells) % buffer.Count;
                return subTableCells;
            }

            //If the subTable is empty we will not paste anything, just skip the buffer index the number of columns in the subTable
            if (subTableCell is not ICollectionCell)
            {
                int columnCount = subTableCell.GetSubTableColumnCount(true);
                bufferIndex = (bufferIndex + columnCount) % buffer.Count;
                return columnCount;
            }
            
            //If the subTable is an empty collection we will not paste anything, just skip the buffer index
            bufferIndex = (bufferIndex + 1) % buffer.Count;
            return 1;
        }

        private static void PasteDictionary(SubTableCell subTableCell, string data, List<string> collectionBuffer)
        {
            //Split the data into keys and values
            int valuesIndex = data.IndexOf(DictionaryValuesStart, StringComparison.Ordinal);
            string keysString = data.Substring(0, valuesIndex);
            string valuesString = data.Substring(valuesIndex);

            List<string> keys = keysString.SplitByLevel(0, CollectionItemStart, CollectionItemEnd).ToList();
            List<string> values = valuesString.SplitByLevel(0, CollectionItemStart, CollectionItemEnd).ToList();

            //Merge the keys and values into a single collection buffer
            for (int j = 0; j < keys.Count; j++)
            {
                collectionBuffer.AddRange(keys[j].Split(CollectionSeparator));
                collectionBuffer.AddRange(values[j].Split(CollectionSeparator));
            }

            Paste(subTableCell.GetImmediateDescendants().ToList(), collectionBuffer, 0);
        }

        private static void PasteCollection(SubTableCell subTableCell, string data, List<string> collectionBuffer)
        {
            //Split the data into rows and merge them into a single collection buffer
            foreach (var row in data.SplitByLevel(0, CollectionItemStart, CollectionItemEnd))
            {
                collectionBuffer.AddRange(row.Split(CollectionSeparator));
            }

            Paste(subTableCell.GetImmediateDescendants().ToList(), collectionBuffer, 0);
        }

        private static string Copy(IList<Cell> cells, bool isInCollection)
        {
            StringBuilder localBuffer = new();

            for (int i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];

                if (i > 0 && cell.GetNearestCommonRow(cells[i - 1]) != null)
                {
                    if (localBuffer.ToString().EndsWith(ColumnSeparator))
                        localBuffer.Length -= ColumnSeparator.Length;
                    localBuffer.Append(RowSeparator);
                }

                if (cell is SubTableCell subTableCell)
                {
                    localBuffer.Append(SerializeSubTable(subTableCell, isInCollection));
                    if (i != cells.Count - 1) localBuffer.Append(ColumnSeparator);
                }
                else
                {
                    string formattedValue = cell.Serialize()
                        .Replace(RowSeparator, CancelledRowSeparator)
                        .Replace(ColumnSeparator, CancelledColumnSeparator);
                    localBuffer.Append(formattedValue);
                    if (i != cells.Count - 1) localBuffer.Append(ColumnSeparator);
                }
            }

            if (isInCollection)
            {
                localBuffer = localBuffer.Replace(ColumnSeparator, CollectionSeparator)
                                         .Replace(RowSeparator, CollectionItemEnd + CollectionItemSeparator + CollectionItemStart);
            }

            return localBuffer.ToString();
        }

        private static string SerializeSubTable(SubTableCell subTableCell, bool isInCollection)
        {
            if (!subTableCell.SubTable.Rows.Any())
            {
                if (subTableCell is ICollectionCell)
                    return "{}";

                return new string(ColumnSeparator[0], subTableCell.GetSubTableColumnCount(true));
            }

            if (subTableCell is DictionaryCell dictionaryCell)
            {
                return DictionaryKeysStart + CollectionItemStart + Copy(dictionaryCell.GetKeys(), true) + CollectionItemEnd +
                       DictionaryValuesStart + CollectionItemStart + Copy(dictionaryCell.GetValues(), true) + CollectionItemEnd;
            }

            if (subTableCell is ICollectionCell)
            {
                return CollectionStart + CollectionItemStart + Copy(subTableCell.GetImmediateDescendants().ToList(), true) + CollectionItemEnd;
            }

            return Copy(subTableCell.GetImmediateDescendants().ToList(), isInCollection);
        }

        private static List<string> SplitBufferIntoPlainList(string buffer)
        {
            List<string> result = new();
            string[] rows = buffer.Split(new[] { RowSeparator }, StringSplitOptions.None);
            foreach (string row in rows)
            {
                result.AddRange(row.Split(new[] { ColumnSeparator }, StringSplitOptions.None));
            }
            return result;
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
