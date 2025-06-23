namespace TableForge.Editor.UI
{
    internal class ListRowAdditionStrategy : IRowAdditionStrategy
    {
        public void AddRow(TableControl tableControl)
        {
            if (tableControl.TableData.ParentCell is ListCell listCell)
            {
                listCell.AddEmptyItem();
                tableControl.SetTable(listCell.SubTable);
            }
        }
    }
}