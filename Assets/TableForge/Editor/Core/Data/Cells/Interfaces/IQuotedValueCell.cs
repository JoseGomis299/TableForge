namespace TableForge.Editor
{
    internal interface IQuotedValueCell
    {
        /// <summary>
        /// Returns the serialized value between quotes.
        /// </summary>
        public string SerializeQuotedValue(bool escapeInternalQuotes);
    }
}