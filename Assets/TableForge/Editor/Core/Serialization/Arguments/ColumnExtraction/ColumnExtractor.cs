using System.Collections.Generic;

namespace TableForge.Editor
{
    internal abstract class ColumnExtractor
    {
        public abstract List<string> ExtractColumnNames(TableDeserializationArgs args);
        public abstract List<List<string>> ExtractColumnData(TableDeserializationArgs args);
    }
}

