using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TableForge.Editor.UI
{
    internal static class TypeRegistry
    {
        private static readonly Dictionary<string, Dictionary<string, Type>> _typesByNamespaceAndName = new(); 
        private static readonly Dictionary<string, HashSet<string>> _typeNamesByNamespace = new();
        private static readonly Dictionary<string, HashSet<Type>> _namespaceTypes = new();
        private static List<string> _namespaces = new();
        
        public static IReadOnlyList<string> Namespaces => _namespaces;
        public static IReadOnlyDictionary<string, Dictionary<string, Type>> TypesByNamespaceAndName => _typesByNamespaceAndName;
        public static IReadOnlyDictionary<string, HashSet<string>> TypeNamesByNamespace => _typeNamesByNamespace;
        public static IReadOnlyDictionary<string, HashSet<Type>> NamespaceTypes => _namespaceTypes;
        public static HashSet<string> TypeNames { get; } = new HashSet<string>();

        static TypeRegistry()
        {
            InitializeNamespaces();
            InitializeTypes();   
        }
        
        private static void InitializeTypes()
        {
            _typesByNamespaceAndName.Clear();

            foreach (var namespaceTypesPair in _namespaceTypes)
            {
                string namespaceName = namespaceTypesPair.Key;
                HashSet<Type> types = namespaceTypesPair.Value;
                
                if (!_typesByNamespaceAndName.ContainsKey(namespaceName))
                {
                    _typesByNamespaceAndName[namespaceName] = new Dictionary<string, Type>();
                    _typeNamesByNamespace[namespaceName] = new HashSet<string>();
                }
                
                foreach (var t in types)
                {
                    _typesByNamespaceAndName[namespaceName][t.Name] = t;
                    _typeNamesByNamespace[namespaceName].Add(t.Name);
                    TypeNames.Add(t.Name);
                }
            }
        }
        
        private static void InitializeNamespaces()
        {
            var namespaceSet = new HashSet<string>();
            bool globalNamespace = false;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if(IsTypeInvalid(type)) continue;

                    string assetNamespace = type.Namespace;
                    if (string.IsNullOrEmpty(assetNamespace))
                    {
                        globalNamespace = true;
                        _namespaceTypes.TryAdd("Global", new HashSet<Type>());
                        _namespaceTypes["Global"].Add(type);
                    }
                    else
                    {
                        namespaceSet.Add(assetNamespace);
                        _namespaceTypes.TryAdd(assetNamespace, new HashSet<Type>());
                        _namespaceTypes[assetNamespace].Add(type);
                    }
                }
            }

            _namespaces = namespaceSet.OrderBy(n => n).ToList();
            if (globalNamespace) _namespaces.Insert(0, "Global");
        }
        
        private static bool IsTypeInvalid(Type type)
        {
            return !type.IsSubclassOf(typeof(ScriptableObject)) ||
                   type.IsAbstract || type.IsGenericType ||
                   type.Assembly == Assembly.GetAssembly(typeof(TypeRegistry)) ||
                   type.IsNotPublic ||
                   IsUnityType(type);
        }

        private static bool IsUnityType(Type type)
        {
            string assemblyName = type.Assembly.GetName().Name;
            return assemblyName.StartsWith("Unity")|| assemblyName.StartsWith("UnityEngine") || assemblyName.StartsWith("UnityEditor");
        }

    }
}