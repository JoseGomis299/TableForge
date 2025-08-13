using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace TableForge.Editor.UI
{
    internal class FunctionExecutor
    {
        private readonly TableControl _tableControl;
        private readonly FunctionParser _functionParser;
        private readonly Dictionary<string, Func<object>> _cachedFunctions;
        private readonly HashSet<int> _incorrectCells = new HashSet<int>();

        public FunctionExecutor(TableControl tableControl)
        {
            _tableControl = tableControl;
            _functionParser = new FunctionParser();
            _cachedFunctions = new Dictionary<string, Func<object>>();
        }

        public void Setup()
        {
            var functions = _tableControl.Metadata.GetFunctions();
            if (functions == null || functions.Count == 0) return;
         
            _cachedFunctions.Clear();
            
            Table table = _tableControl.TableData;
            foreach (var function in functions.Values)
            {
                if (!_cachedFunctions.ContainsKey(function))
                {
                    var cellFunction = _functionParser.ParseCellFunction(function, table);
                    if (cellFunction != null)
                        _cachedFunctions[function] = cellFunction;
                }
            }
        }
        
        public void SetCellFunction(Cell cell, string function)
        {
            int cellId = cell.Id;
            string oldFunction = _tableControl.Metadata.GetFunction(cellId);
            if(function == oldFunction)
            {
                return; // No change, nothing to do
            }
            
            SetFunctionCommand command = new SetFunctionCommand(cellId, function, oldFunction, _tableControl);
            if(UndoRedoManager.GetLastUndoCommand() is SetFunctionCommand lastCommand && lastCommand.BoundCell.Id == cell.Id)
            {
                lastCommand.Combine(command);
            }
            else UndoRedoManager.Do(command);
            
            if (!_cachedFunctions.ContainsKey(function))
            {
                var cellFunction = _functionParser.ParseCellFunction(function, _tableControl.TableData);
                if (cellFunction != null)
                    _cachedFunctions[function] = cellFunction;
            }
        }
        
        public void ExecuteCellFunction(int cellId)
        {
            var function = GetFunction(cellId);
            Cell cell = Editor.CellExtension.GetCellById(_tableControl.TableData, cellId);

            if (function != null && cell != null)
            {
                object result = null;
                
                try
                { 
                    result = function.Invoke();
                    if(cell.Type.IsAssignableFrom(result.GetType()))
                    {
                        cell.SetValue(result);
                        return;
                    }
                    
                    object properTypeResult = Convert.ChangeType(result, cell.Type, CultureInfo.InvariantCulture);
                    cell.SetValue(properTypeResult);
                }
                catch (Exception e)
                {
                    _incorrectCells.Add(cellId);
                    Debug.LogError($"Function evaluation error in cell {cell.GetGlobalPosition()} for input: {_tableControl.Metadata.GetFunction(cellId)}\n" +
                                   $"Expected type: {cell.Type}, but got: {result?.GetType()}\n" +
                                   $"Error: {e.Message}");
                }
            }
        }

        public void ExecuteAllFunctions()
        {
            if(_tableControl == null || _tableControl.Metadata == null)
                return;
            
            _incorrectCells.Clear();
            
            // Use bfs to execute functions in the correct order
            Queue<FunctionNode> queue = BuildExecutionTree();
            HashSet<int> executedIds = new HashSet<int>();
            while (queue.Count > 0)
            {
                FunctionNode currentNode = queue.Dequeue();
                
                if (executedIds.Contains(currentNode.Id)) continue; // Skip already executed nodes
                
                // Execute the function for the current node
                ExecuteCellFunction(currentNode.Id);
                executedIds.Add(currentNode.Id);
                
                // Enqueue children to process them later
                foreach (var child in currentNode.Children)
                {
                    // Only enqueue children that are one level deeper
                    if (!executedIds.Contains(child.Id) && child.Depth == currentNode.Depth + 1) 
                    {
                        queue.Enqueue(child);
                    }
                }
            }
            
            _tableControl.Update();
        }
        
        public bool IsCellFunctionCorrect(int cellId)
        {
            return string.IsNullOrEmpty(_tableControl.Metadata.GetFunction(cellId)) || !_incorrectCells.Contains(cellId);
        }

        private Func<object> GetFunction(int id)
        {
            string function = _tableControl.Metadata.GetFunction(id);
            if (string.IsNullOrEmpty(function))
            {
                return null;
            }
            
            if (_cachedFunctions.TryGetValue(function, out var cachedFunction))
            {
                return cachedFunction;
            }
            
            var parsedFunction = _functionParser.ParseCellFunction(function, _tableControl.TableData);
            if (parsedFunction != null)
            {
                _cachedFunctions[function] = parsedFunction;
                return parsedFunction;
            }

            return null;
        }

        private Queue<FunctionNode> BuildExecutionTree()
        {
            var executionTree = new Queue<FunctionNode>();
            var functions = _tableControl.Metadata.GetFunctions();

            Dictionary<int, FunctionNode> nodes = new Dictionary<int, FunctionNode>();
            foreach (var idFunctionPair in functions)
            {
                int id = idFunctionPair.Key;
                string function = idFunctionPair.Value;
                if (string.IsNullOrEmpty(function)) continue; // Skip already processed functions

                List<string> referencedCellPositions = ReferenceParser.ExtractReferences(function);
                List<Cell> referencedCells = referencedCellPositions.SelectMany(s => ReferenceParser.ResolveReference(s, _tableControl.TableData))
                    .Where(c => c != null).ToList();

                if (!nodes.TryGetValue(id, out var node))
                {
                    node = new FunctionNode(id);
                    nodes[id] = node;
                }

                foreach (var cell in referencedCells)
                {
                    if (string.IsNullOrEmpty(_tableControl.Metadata.GetFunction(cell.Id)))
                    {
                        continue; // Skip non-existing cells
                    }
                    
                    if (!nodes.TryGetValue(cell.Id, out var parentNode))
                    {
                        parentNode = new FunctionNode(cell.Id);
                        nodes[cell.Id] = parentNode;
                    }
                    
                    if(node == parentNode) 
                    {
                        _incorrectCells.Add(node.Id);
                        Debug.LogError($"Circular dependency detected involving cell \'{Editor.CellExtension.GetCellById(_tableControl.TableData, node.Id)?.GetGlobalPosition()}\' with function: {_tableControl.Metadata.GetFunction(node.Id)}");
                        continue;
                    }

                    try
                    {
                        node.AddParent(parentNode);
                    }
                    catch (Exception)
                    {
                        _incorrectCells.Add(node.Id);
                        _incorrectCells.UnionWith(node.GetDescendants().Select(p => p.Id));
                        Debug.LogError($"Circular dependency detected involving cell \'{Editor.CellExtension.GetCellById(_tableControl.TableData, node.Id)?.GetGlobalPosition()}\' with function: {_tableControl.Metadata.GetFunction(node.Id)}");
                    }
                }
            }

            // Detect circular dependencies
            foreach (var node in nodes.Values)
            {
                if(_incorrectCells.Contains(node.Id)) continue; // Skip already marked nodes
                
                if (node.Depth == 0) // Root node
                {
                    executionTree.Enqueue(node);
                }
            }

            return executionTree;
        }

        private class FunctionNode
        {
            private readonly List<FunctionNode> _children;
            private readonly List<FunctionNode> _parents;
            public int Id { get; }
            public int Depth { get; private set; }
            public IReadOnlyList<FunctionNode> Children => _children;
            public IReadOnlyList<FunctionNode> Parents => _parents;

            public FunctionNode(int id)
            {
                _children = new List<FunctionNode>();
                _parents = new List<FunctionNode>();
                Depth = 0;
                Id = id;
            }
            
            public void AddChild(FunctionNode child)
            {
                if (child == null || child == this) return; // Avoid self-references
                
                _children.Add(child);
                child.AddParent(this);
            }
            
            public void AddParent(FunctionNode parent)
            {
                if (parent == null || parent == this) return; // Avoid self-references
                
                if(IsAncestorOf(parent)) 
                    throw new ArgumentException($"Circular dependency detected.");
                
                _parents.Add(parent);
                parent._children.Add(this);
                Depth = Math.Max(Depth, parent.Depth + 1);

                foreach (var child in _children)
                {
                    child.RecalculateDepth();
                }
            }
            
            public IReadOnlyList<FunctionNode> GetAncestors()
            {
                List<FunctionNode> ancestors = new List<FunctionNode>();
                GetAncestorsRecursive(ancestors);
                return ancestors;
            }
            
            public IReadOnlyList<FunctionNode> GetDescendants()
            {
                List<FunctionNode> descendants = new List<FunctionNode>();
                GetDescendantsRecursive(descendants);
                return descendants;
            }
            
            private void GetDescendantsRecursive(List<FunctionNode> descendants)
            {
                foreach (var child in _children)
                {
                    if (!descendants.Contains(child))
                    {
                        descendants.Add(child);
                        child.GetDescendantsRecursive(descendants);
                    }
                }
            }
            
            private void GetAncestorsRecursive(List<FunctionNode> ancestors)
            {
                foreach (var parent in _parents)
                {
                    if (!ancestors.Contains(parent))
                    {
                        ancestors.Add(parent);
                        parent.GetAncestorsRecursive(ancestors);
                    }
                }
            }
            
            private void RecalculateDepth()
            {
                Depth = _parents.Max(p => p.Depth) + 1;
                foreach (var child in _children)
                {
                    child.RecalculateDepth();
                }
            }
            
            private bool IsAncestorOf(FunctionNode node)
            {
                if (node == null) return false;
                if (node == this) return true;

                foreach (var child in _children)
                {
                    if (child.IsAncestorOf(node)) return true;
                }

                return false;
            }
        }
    }
}