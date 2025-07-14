using System;
using System.Collections.Generic;
using System.Linq;

namespace TableForge.Editor
{
    /// <summary>
    /// Defines a strategy for determining the cell mapped to a given type.
    /// </summary>
    internal interface ICellMappingStrategy
    {
        /// <summary>
        /// Attempts to determine the appropriate cell type for the given type.
        /// </summary>
        /// <param name="type">The type to evaluate.</param>
        /// <param name="cellMappings">A dictionary of type match modes and their corresponding cell type mappings.</param>
        /// <param name="cellType">The determined cell type, if found.</param>
        /// <returns>True if a matching cell type is found; otherwise, false.</returns>
        bool TryGetCellType(Type type, Dictionary<TypeMatchMode, List<(HashSet<Type> SupportedTypes, Type cellType)>> cellMappings, out Type cellType);
    }

    internal abstract class BaseCellMappingStrategy : ICellMappingStrategy
    {
        public abstract bool TryGetCellType(Type type, Dictionary<TypeMatchMode, List<(HashSet<Type> SupportedTypes, Type cellType)>> cellMappings, out Type cellType);
        
        protected static bool TryGetMappings(TypeMatchMode matchMode, Dictionary<TypeMatchMode, List<(HashSet<Type> SupportedTypes, Type cellType)>> cellMappings, out List<(HashSet<Type> SupportedTypes, Type cellType)> mappings)
        {
            return cellMappings.TryGetValue(matchMode, out mappings) && mappings.Count > 0;
        }
    }

    /// <summary>
    /// Strategy for exact type matches.
    /// </summary>
    internal class ExactMatchStrategy : BaseCellMappingStrategy
    {
        public override bool TryGetCellType(Type type, Dictionary<TypeMatchMode, List<(HashSet<Type> SupportedTypes, Type cellType)>> cellMappings, out Type cellType)
        {
            if (TryGetMappings(TypeMatchMode.Exact, cellMappings, out var mappings))
            {
                var match = mappings.FirstOrDefault(mapping => mapping.SupportedTypes.Contains(type));
                if (match.cellType != null)
                {
                    cellType = match.cellType;
                    return true;
                }
            }

            cellType = null;
            return false;
        }
    }

    /// <summary>
    /// Strategy for types that are assignable to the registered types.
    /// </summary>
    internal class AssignableMatchStrategy : BaseCellMappingStrategy
    {
        public override bool TryGetCellType(Type type, Dictionary<TypeMatchMode, List<(HashSet<Type> SupportedTypes, Type cellType)>> cellMappings, out Type cellType)
        {
            if (TryGetMappings(TypeMatchMode.Assignable, cellMappings, out var mappings))
            {
                var match = mappings.FirstOrDefault(mapping => mapping.SupportedTypes.Any(t => t.IsAssignableFrom(type)));
                if (match.cellType != null)
                {
                    cellType = match.cellType;
                    return true;
                }
            }

            cellType = null;
            return false;
        }
    }

    /// <summary>
    /// Strategy for matching generic types based on their definitions.
    /// </summary>
    internal class GenericMatchStrategy : BaseCellMappingStrategy
    {
        public override bool TryGetCellType(Type type, Dictionary<TypeMatchMode, List<(HashSet<Type> SupportedTypes, Type cellType)>> cellMappings, out Type cellType)
        {
            if (type.IsGenericType && TryGetMappings(TypeMatchMode.GenericArgument, cellMappings, out var mappings))
            {
                var match = mappings.FirstOrDefault(mapping => mapping.SupportedTypes.Any(t => t.IsGenericTypeDefinition && t == type.GetGenericTypeDefinition()));
                if (match.cellType != null)
                {
                    cellType = match.cellType;
                    return true;
                }
            }

            cellType = null;
            return false;
        }
    }
}
