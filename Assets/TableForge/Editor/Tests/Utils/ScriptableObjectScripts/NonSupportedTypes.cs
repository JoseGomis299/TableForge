using System.Collections.Generic;
using UnityEngine;

namespace TableForge.Tests
{
    internal class NonSupportedTypes : ScriptableObject
    {
        public LinkedList<int> linkedListField;
        public object objectField;
        public Queue<int> queueField;
        public Stack<int> stackField;
        public SortedList<int, string> sortedListField;
        public HashSet<int> hashSetField;
        public Dictionary<int, int> dictionaryField;
    }
}