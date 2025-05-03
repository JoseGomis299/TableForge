namespace TableForge.UI
{
    internal interface ISimpleCellControl
    {
        void FocusField();
        void BlurField();
        bool IsFieldFocused();
    }
}