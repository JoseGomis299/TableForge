using System.Collections.Generic;

namespace TableForge.UI
{
    internal class DefaultRowDeletionStrategy : IRowDeletionStrategy
    {
        public void DeleteRow(TableControl tableControl)
        {
            if(tableControl.TableData.Rows.Count == 0) return;

            var selectedRows = tableControl.CellSelector.GetSelectedRows();
            List<Row> rowsToDelete = new List<Row>();
            foreach (var row in selectedRows)
            {
                if (row.Table != tableControl.TableData) continue;
                rowsToDelete.Add(row);
            }
            rowsToDelete.Sort((a, b) => b.Position.CompareTo(a.Position));
            
            if(rowsToDelete.Count == 0) tableControl.TableData.RemoveRow(tableControl.TableData.Rows.Count);
            else
            {
                foreach (var row in rowsToDelete)
                {
                    tableControl.RemoveRow(row.Id);
                }
            }
            tableControl.RebuildPage();
        }
    }
}