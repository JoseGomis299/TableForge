namespace TableForge.Editor.UI
{
    public enum TableReorderMode
    {
        /// <summary>
        ///  No reordering will be done
        /// </summary>
        None,
        /// <summary>
        ///  If the implemented internal reorder changes the visual order of the elements, this should be used.
        ///  <example>List or array reorderings</example>
        /// </summary>
        ImplicitReorder, 
        /// <summary>
        /// If the implemented internal reorder does not change the visual order of the elements, this should be used.
        /// <example>Reorderings which affect only visually</example>
        /// </summary>
        ExplicitReorder 
    }
}