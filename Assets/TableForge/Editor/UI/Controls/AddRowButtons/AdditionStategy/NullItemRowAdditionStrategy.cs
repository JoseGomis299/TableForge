namespace TableForge.UI
{
    internal class NullItemRowAdditionStrategy : IRowAdditionStrategy
    {
        private TableControl _tableControl;
        public NullItemRowAdditionStrategy(TableControl tableControl)
        {
            _tableControl = tableControl;
        }
        
        public void AddRow()
        {
            if(_tableControl.TableData.ParentCell is SubItemCell nullItemCell)
                nullItemCell.CreateDefaultValue();
            
            _tableControl.RebuildPage();
        }
    }
}