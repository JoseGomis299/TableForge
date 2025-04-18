namespace TableForge.UI
{
    internal interface ICellNavigator
    {
        public Cell GetCurrentCell();
        public Cell GetNextCell(int orientation);
    }
}