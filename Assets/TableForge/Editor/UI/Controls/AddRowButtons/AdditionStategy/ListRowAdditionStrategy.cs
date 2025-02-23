namespace TableForge.UI
{
    internal class ListRowAdditionStrategy : IRowAdditionStrategy
    {
        private TableControl _tableControl;
        public ListRowAdditionStrategy(TableControl tableControl)
        {
            _tableControl = tableControl;
        }
        
        public void AddRow()
        {
            if(_tableControl.TableData.ParentCell is ListCell listCell)
                listCell.AddEmptyItem();
            
            _tableControl.RebuildPage();
        }
    }
}