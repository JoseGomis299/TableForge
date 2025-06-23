namespace TableForge.Editor.UI
{
    internal interface ITextBasedCellControl : ISimpleCellControl
    {
        void SetValue(string value, bool focus);
        string GetValue();
    }
}