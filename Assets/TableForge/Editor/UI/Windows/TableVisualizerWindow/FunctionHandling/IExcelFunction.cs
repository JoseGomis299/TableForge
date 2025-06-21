using System;
using System.Collections.Generic;

namespace TableForge.UI
{
    internal interface IExcelFunction
    {
        IReadOnlyList<ArgumentDefinition> ExpectedArguments { get; }
        string Name { get; }
        bool ValidateArguments(List<object> args);
        object Evaluate(List<object> args, FunctionContext context);
    }
    
    [Flags]
    internal enum ArgumentType
    {
        Numeric = 1,
        String = 2,
        LogicExpression = 4,
        Criteria = 8,
        Range = 16,
        CellReference = 32,
        Function = 64,
        Reference = Range | CellReference,
        Number = Numeric | Range | CellReference | Function,
        Text = String | Criteria,
        Value = Numeric | String | Range | CellReference | Function,
        Any = Numeric | String | LogicExpression | Criteria | Range | CellReference | Function
    }
    
    internal struct ArgumentDefinition
    {
        public ArgumentType Type;
        public bool IsOptional;
        public bool IndefiniteArguments;

        public ArgumentDefinition(ArgumentType type, bool isOptional = false, bool indefiniteArguments = false)
        {
            Type = type;
            IsOptional = isOptional;
            IndefiniteArguments = indefiniteArguments;
        }
    }

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
            { ArgumentType.Function, new HashSet<Type> { typeof(string) } }
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

    internal readonly struct ArgumentDefinitionCollection
    {
        private readonly List<ArgumentDefinition> _definitions;
        
        public IReadOnlyList<ArgumentDefinition> Definitions => _definitions;
        
        public ArgumentDefinitionCollection(List<ArgumentDefinition> definitions)
        {
            _definitions = definitions ?? new List<ArgumentDefinition>();
        }

        public bool ValidateArguments(List<object> arguments)
        {
            int requiredCount = GetRequiredCount();
            if (arguments.Count < requiredCount)
            {
                return false; // Not enough arguments
            }

            for (int i = 0; i < arguments.Count; i++)
            {
                if(arguments[i] == null)
                {
                    return false; // Null argument is not allowed
                }

                int definitionIndex = i;
                if (i >= _definitions.Count)
                {
                    if (!_definitions[^1].IndefiniteArguments)
                    {
                        return false; // Too many arguments and last argument is not indefinite
                    }
                    
                    definitionIndex = _definitions.Count - 1; // Use the last definition for indefinite arguments
                }

                var definition = _definitions[definitionIndex];
                var argType = arguments[i].GetType();

                if (!ArgumentTypeMapper.IsValidType(definition.Type, argType))
                {
                    return false; // Argument type does not match expected type
                }
                
            }

            return true;
        }
        
        private int GetRequiredCount()
        {
            int requiredCount = 0;

            foreach (var definition in _definitions)
            {
                if (!definition.IsOptional)
                {
                    requiredCount++;
                }
            }

            return requiredCount;
        }
    }
}