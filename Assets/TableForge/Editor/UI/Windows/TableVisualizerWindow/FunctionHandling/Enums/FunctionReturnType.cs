using System;

namespace TableForge.Editor.UI
{
    [Flags]
    internal enum FunctionReturnType
    {
        Number = 1,
        String = 2,
        Boolean = 4,
        Any = Number | String | Boolean,
        Value = Number | String,
    }
}