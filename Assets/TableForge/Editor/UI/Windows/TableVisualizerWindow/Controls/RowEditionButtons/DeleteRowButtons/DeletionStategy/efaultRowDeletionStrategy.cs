namespace TableForge.Editor.UI
{
    internal class RowDeletionStrategy : IRowDeletionStrategy
    {
        public void DeleteRow(TableControl tableControl)
        {
            if(tableControl.TableData.Rows.Count == 0) return;

            var rowsToDelete = tableControl.CellSelector.GetSelectedRows(tableControl.TableData);
            rowsToDelete.Sort((a, b) => b.Position.CompareTo(a.Position));
            
            if(rowsToDelete.Count == 0) tableControl.RemoveRow(tableControl.TableData.Rows[tableControl.TableData.Rows.Count].Id);
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