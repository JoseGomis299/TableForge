namespace TableForge.UI
{
    internal class DefaultRowDeletionStrategy : IRowDeletionStrategy
    {
        public void DeleteRow(TableControl tableControl)
        {
            if(tableControl.TableData.Rows.Count == 0) return;
            
            tableControl.TableData.RemoveRow(tableControl.TableData.Rows.Count);
            tableControl.RebuildPage();
        }
    }
}