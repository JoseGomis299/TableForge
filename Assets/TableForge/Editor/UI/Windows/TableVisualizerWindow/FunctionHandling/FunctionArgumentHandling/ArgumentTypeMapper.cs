using System;
using System.Collections.Generic;

namespace TableForge.Editor.UI
{
    internal static class ArgumentTypeMapper
    {
        private static readonly Dictionary<ArgumentType, HashSet<Type>> _typeMappings = new()
        {
            { ArgumentType.Numeric, new HashSet<Type> { typeof(double), typeof(string), typeof(int), typeof(float), typeof(ulong) } },
            { ArgumentType.String, new HashSet<Type> { typeof(string) } },
            { ArgumentType.LogicExpression, new HashSet<Type> { typeof(bool) } },
            { ArgumentType.Criteria, new HashSet<Type> { typeof(Func<object, bool>) } },
            { ArgumentType.Range, new HashSet<Type> { typeof(List<Cell>) } },
            { ArgumentType.CellReference, new HashSet<Type> { typeof(List<Cell>), typeof(Cell) } },
            { ArgumentType.StringFunction, new HashSet<Type> { typeof(string) } },
            { ArgumentType.LogicalFunction, new HashSet<Type> { typeof(string) } },
        };
        
        public static bool IsValidType(ArgumentType argumentType, Type valueType)
        {
            List<ArgumentType> decomposedType = new List<ArgumentType>();
            int typeValue = (int)argumentType;
            int i = 0;
            while (1 << i <= typeValue)
            {
                if ((typeValue & (1 << i)) != 0)
                {
                    decomposedType.Add((ArgumentType)(1 << i));
                }
                i++;
            }
            
            // Check if the valueType matches any of the decomposed argument types
            foreach (var type in decomposedType)
            {
                if (_typeMappings.TryGetValue(type, out var validTypes) && validTypes.Contains(valueType))
                {
                    return true;
                }
            }
            
            return false;
        }
    }
}