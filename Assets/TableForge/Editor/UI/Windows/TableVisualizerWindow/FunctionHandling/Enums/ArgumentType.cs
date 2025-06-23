using System;

namespace TableForge.Editor.UI
{
    [Flags]
    internal enum ArgumentType
    {
        Numeric = 1,
        String = 2,
        LogicExpression = 4,
        Criteria = 8,
        Range = 16,
        CellReference = 32,
        ValueFunction = 64,
        LogicalFunction = 128,
        Boolean = LogicExpression | LogicalFunction | CellReference,
        Reference = Range | CellReference,
        Number = Numeric | Range | CellReference | ValueFunction,
        Text = String | Criteria,
        Value = Numeric | String | Range | CellReference | ValueFunction,
        Any = Numeric | String | LogicExpression | Criteria | Range | CellReference | ValueFunction | LogicalFunction
    }
}