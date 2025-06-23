using System.Collections.Generic;

namespace TableForge.Editor.UI
{
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
                    if (!_definitions[^1].indefiniteArguments)
                    {
                        return false; // Too many arguments and last argument is not indefinite
                    }
                    
                    definitionIndex = _definitions.Count - 1; // Use the last definition for indefinite arguments
                }

                var definition = _definitions[definitionIndex];
                var argType = arguments[i].GetType();

                if (!ArgumentTypeMapper.IsValidType(definition.type, argType))
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
                if (!definition.isOptional)
                {
                    requiredCount++;
                }
            }

            return requiredCount;
        }

        public override string ToString()
        {
            string result = "";
            for (var index = 0; index < _definitions.Count; index++)
            {
                var definition = _definitions[index];
                result += definition.ToString();
                if (index < _definitions.Count - 1)
                {
                    result += "; ";
                }
            }

            return result;
        }
    }
}