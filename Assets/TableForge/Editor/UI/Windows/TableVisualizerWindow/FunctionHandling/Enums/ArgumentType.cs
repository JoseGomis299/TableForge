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
        StringFunction = 64,
        LogicalFunction = 128,
        ArithmeticOperation = 256,
        NumericFunction = 512,

        Boolean = LogicExpression | LogicalFunction | CellReference,
        Reference = Range | CellReference,
        Number = Numeric | Range | CellReference | NumericFunction | ArithmeticOperation,
        SingleNumber = Numeric | CellReference | NumericFunction | ArithmeticOperation,
        Text = String | Criteria,
        Value = Numeric | String | Range | CellReference | StringFunction | ArithmeticOperation | NumericFunction,
        Function = StringFunction | LogicalFunction | NumericFunction,
        Any = Numeric | String | LogicExpression | Criteria | Range | CellReference | StringFunction | LogicalFunction | ArithmeticOperation | NumericFunction
    }
}