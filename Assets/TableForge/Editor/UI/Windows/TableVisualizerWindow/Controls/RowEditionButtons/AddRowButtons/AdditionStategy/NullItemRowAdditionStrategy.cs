namespace TableForge.Editor.UI
{
    internal class NullItemRowAdditionStrategy : IRowAdditionStrategy
    {
        public void AddRow(TableControl tableControl)
        {
            if(tableControl.TableData.ParentCell is SubItemCell nullItemCell)
                nullItemCell.CreateDefaultValue();
            
            tableControl.RebuildPage();
        }
    }
}