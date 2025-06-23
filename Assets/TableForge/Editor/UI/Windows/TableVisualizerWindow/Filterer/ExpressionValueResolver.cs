using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace TableForge.Editor.UI.TableForge.UI
{
    internal class ExpressionValueResolver
    {
        private readonly TableControl _tableControl;

        public ExpressionValueResolver(TableControl tableControl)
        {
            _tableControl = tableControl;
        }

        public object GetCellValue(Row row, string columnRef)
        {
            if (string.IsNullOrEmpty(columnRef) || char.IsDigit(columnRef[0]) || columnRef[0] == '\"' || columnRef[0] == '\'')
                return null;

            Column column;
            Cell cell;

            if (columnRef.Contains('.'))
            {
                var values = new List<object>();
                var parts = columnRef.Split('.');

                column = GetColumn(parts[0], _tableControl.TableData);
                if (column == null) return null;

                cell = row.Cells[column.Position];
                if (cell is SubTableCell subTableCell)
                {
                    if (!RetrieveNestedValues(subTableCell.SubTable, parts, 1, values))
                        return null;

                    if (values.Count == 0) return null;
                    if (values.Count == 1)
                    {
                        if (values[0] == null || (values[0] is Object o && o == null)) return "null";
                        return values[0];
                    }
                    return values;
                }

                return null;
            }

            column = GetColumn(columnRef, _tableControl.TableData);
            if (column == null) return null;

            cell = row.Cells[column.Position];
            object value = cell?.GetValue();
            if (value == null || (value is Object obj && obj == null)) return "null";
            return value;
        }

        public object GetListValue(string stringList)
        {
            if (string.IsNullOrEmpty(stringList) || (!stringList.StartsWith('[') && !stringList.EndsWith(']')))
                return stringList;

            stringList = stringList.Trim('[', ']');
            var values = new List<object>();
            var items = stringList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in items)
            {
                var trimmedItem = item.Trim();
                if ((trimmedItem.StartsWith("\"") && trimmedItem.EndsWith("\"")) ||
                    (trimmedItem.StartsWith("'") && trimmedItem.EndsWith("'")))
                {
                    values.Add(trimmedItem[1..^1]); // Substring without first/last char
                }
                else
                {
                    if (TryParseNumber(trimmedItem, out double number))
                        values.Add(number);
                    else
                        values.Add(GetListValue(trimmedItem));
                }
            }
            return values;
        }

        public bool TryParseNumber(object value, out double result)
        {
            result = 0;
            if (value == null) return false;

            if (value is string s) value = s.Replace('.', ',');
            return double.TryParse(value.ToString(), out result);
        }

        private bool RetrieveNestedValues(Table table, string[] parts, int index, List<object> values)
        {
            var part = parts[index];
            Column column = GetColumn(part, table);
            if (column == null) return false;

            for (int i = 1; i <= table.Rows.Count; i++)
            {
                var cell = table.GetCell(column.Position, i);
                if (cell == null) return false;

                if (index == parts.Length - 1)
                {
                    values.Add(cell.GetValue());
                }
                else if (cell is SubTableCell subTableCell)
                {
                    if (!RetrieveNestedValues(subTableCell.SubTable, parts, index + 1, values))
                        return false;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private Column GetColumn(string reference, Table table)
        {
            if (reference.StartsWith("$") && reference.Length == 2 && char.IsLetter(reference[1]))
            {
                return table.Columns.GetValueOrDefault(PositionUtil.ConvertToNumber(reference[1].ToString()));
            }

            return table.ColumnsByName.GetValueOrDefault(reference);
        }
    }
}