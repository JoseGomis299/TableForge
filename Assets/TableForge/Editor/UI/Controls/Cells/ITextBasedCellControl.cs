namespace TableForge.UI
{
    internal interface ITextBasedCellControl : ISimpleCellControl
    {
        void SetValue(string value, bool focus);
    }
}