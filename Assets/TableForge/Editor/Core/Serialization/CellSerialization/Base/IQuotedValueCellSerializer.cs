using TableForge.Editor.Serialization;

namespace TableForge.Editor
{
    internal interface IQuotedValueCellSerializer : ICellSerializer
    {
        /// <summary>
        /// Returns the serialized value between quotes.
        /// </summary>
        public string SerializeQuotedValue(SerializationOptions options, bool escapeInternalQuotes);
    }
}