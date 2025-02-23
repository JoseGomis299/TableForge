namespace TableForge.UI
{
    public enum TableReorderMode
    {
        None,
        ImplicitReorder, //If the implemented internal reorder changes the visual order of the elements, this should be used
        ExplicitReorder //If the implemented internal reorder does not change the visual order of the elements, this should be used
    }
}