using System.Collections.Generic;
using UnityEngine;

namespace TableForge.Tests
{
    internal class NonSupportedTypes : ScriptableObject
    {
        [TableForgeSerialize] public LinkedList<int> linkedListField;
        [TableForgeSerialize] public object objectField;
        [TableForgeSerialize] public Queue<int> queueField;
        [TableForgeSerialize] public Stack<int> stackField;
        [TableForgeSerialize] public SortedList<int, string> sortedListField;
        [TableForgeSerialize] public HashSet<int> hashSetField;
    }
}