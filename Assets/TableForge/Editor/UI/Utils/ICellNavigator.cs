namespace TableForge.Editor.UI
{
    internal interface ICellNavigator
    {
        public Cell GetCurrentCell();
        public Cell GetNextCell(int orientation);
        public void SetCurrentCell(Cell cell);
    }
}