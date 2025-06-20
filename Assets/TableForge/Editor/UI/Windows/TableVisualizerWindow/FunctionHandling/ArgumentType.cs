using System;

namespace TableForge.UI
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
        NumericFunction = 64,
        LogicalFunction = 128,
        Boolean = LogicExpression | LogicalFunction | CellReference,
        Reference = Range | CellReference,
        Number = Numeric | Range | CellReference | NumericFunction,
        Text = String | Criteria,
        Value = Numeric | String | Range | CellReference | NumericFunction,
        Any = Numeric | String | LogicExpression | Criteria | Range | CellReference | NumericFunction | LogicalFunction
    }
}