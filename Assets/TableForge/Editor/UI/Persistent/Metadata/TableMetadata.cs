using System;
using System.Collections.Generic;
using System.Linq;
using TableForge.DataStructures;
using UnityEditor;
using UnityEngine;

namespace TableForge.Editor.UI
{
    /// <summary>
    /// Manages metadata for table display and behavior, including visibility states, positions, sizes, and functions.
    /// This ScriptableObject persists table configuration across Unity sessions.
    /// </summary>
    internal class TableMetadata : ScriptableObject
    {
        #region Fields
        
        // Visibility and state tracking
        [HideInInspector] [SerializeField] private SerializedHashSet<int> expandedTables = new();
        [HideInInspector] [SerializeField] private SerializedHashSet<int> transposedTables = new();
        [HideInInspector] [SerializeField] private SerializedHashSet<int> hiddenFields = new();
            
        // Type and binding information
        [HideInInspector] [SerializeField] private string itemsTypeName;
        [HideInInspector] [SerializeField] private string bindingTypeName;
        [HideInInspector] [SerializeField] private SerializedHashSet<string> itemGUIDs = new();
        
        // Layout and function data
        [HideInInspector] [SerializeField] private SerializedDictionary<int, CellAnchorMetadata> cellAnchorMetadata = new();
        [HideInInspector] [SerializeField] private SerializedDictionary<int, string> functions = new();

        #endregion

        #region Properties

        public string Name
        {
            get => name;
            set
            {
                if (string.IsNullOrEmpty(value) || value == name) return;
                
                this.Rename(value);
                name = value;
                SetDirtyIfNecessary();
            }
        }
        
        public bool IsTransposed
        {
            get => transposedTables.Contains(0);
            set
            {
                if (value) transposedTables.Add(0);
                else transposedTables.Remove(0);
                
                SetDirtyIfNecessary();
            }
        }

        public IReadOnlyList<string> ItemGUIDs
        {
            get
            {
                RemoveNonExistingItemGUIDs();
                return string.IsNullOrEmpty(bindingTypeName) ? 
                    itemGUIDs.Values : 
                    AssetDatabase.FindAssets($"t:{bindingTypeName}").ToList();
            }
        }
        
        public bool ContainsItem(string guid) => IsTypeBound
                ? AssetDatabase.GetMainAssetTypeFromGUID(new GUID(guid)).Name == bindingTypeName
                : itemGUIDs.Contains(guid);
        
        public bool IsTypeBound => !string.IsNullOrEmpty(bindingTypeName);
        public string BindingTypeName => bindingTypeName;
        
        #endregion
        
        #region Getters

        public bool HasGuid(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return false;
            if (IsTypeBound)
            {
                return AssetDatabase.GetMainAssetTypeFromGUID(new GUID(guid))?.Name == bindingTypeName;
            }
            return itemGUIDs.Contains(guid);
        }
        
        public bool HasAnchorData()
        {
            return cellAnchorMetadata is { Count: > 0 };
        }
        
        public bool IsFieldVisible(int anchorId)
        {
            return !hiddenFields.Contains(anchorId);
        }
        
        public bool IsTableExpanded(int subTableCellId)
        {
            return expandedTables.Contains(subTableCellId);
        }
        
        public bool IsTableTransposed(int subTableCellId)
        {
            return transposedTables.Contains(subTableCellId);
        }
        
        public int GetAnchorPosition(int anchorId)
        {
            return cellAnchorMetadata.TryGetValue(anchorId, out var metadata) ? metadata.position : 0;
        }
        
        public Vector2 GetAnchorSize(int anchorId)
        {
            cellAnchorMetadata ??= new SerializedDictionary<int, CellAnchorMetadata>();
            return cellAnchorMetadata.TryGetValue(anchorId, out var metadata) ? metadata.size : Vector2.zero;
        }
        
        public string GetFunction(int cellOrAnchorId)
        {
            return functions.TryGetValue(cellOrAnchorId, out var formula) ? formula : string.Empty;
        }

        public Dictionary<int, string> GetFunctions() => functions;
        
        public Type GetItemsType()
        {
            if (string.IsNullOrEmpty(itemsTypeName)) return null;
            
            Type type = Type.GetType(itemsTypeName);
            if (type == null)
            {
                Debug.LogWarning($"Failed to find type: {itemsTypeName}");
                return null;
            }
            
            return type;
        }
        
        
        #endregion
        
        #region Setters
        public void SetBindingType(Type type)
        {
            if (type == null)
            {
                bindingTypeName = null;
                return;
            }
            
            bindingTypeName = type.Name;
            SetItemsType(type);
            SetDirtyIfNecessary();
        }
        
        public void SetItemGUIDs(Table table)
        {
            itemGUIDs.Clear();
            foreach (var row in table.OrderedRows)
            {
                itemGUIDs.Add(row.SerializedObject.RootObjectGuid);
            }
            
            SetDirtyIfNecessary();
        }
        
        public void SetItemGUIDs(IEnumerable<string> guids)
        {
            itemGUIDs.Clear();
            foreach (var guid in guids)
            {
                itemGUIDs.Add(guid);
            }
            
            SetDirtyIfNecessary();
        }
        
        public void AddItemGuid(string guid)
        {
            itemGUIDs.Add(guid);
            SetDirtyIfNecessary();
        }
        
        public void RemoveItemGuid(string guid)
        {
            itemGUIDs.Remove(guid);
            SetDirtyIfNecessary();
        }
        
        public void SetFunction(int cellOrAnchorId, string formula)
        {
            if (string.IsNullOrEmpty(formula))
            {
                functions.Remove(cellOrAnchorId);
            }
            else
            {
                functions[cellOrAnchorId] = formula;
            }
            
            SetDirtyIfNecessary();
        }
        
        public void SetFieldVisible(int anchorId, bool isVisible)
        {
            if(isVisible) hiddenFields.Remove(anchorId);
            else hiddenFields.Add(anchorId);
            
            SetDirtyIfNecessary();
        }
        
        public void SetTableExpanded(int subTableCellId, bool isExpanded)
        {
            if(isExpanded) expandedTables.Add(subTableCellId);
            else expandedTables.Remove(subTableCellId);
            
            SetDirtyIfNecessary();
        }
        
        public void SetAnchorPosition(int anchorId, int position)
        {
            if (!cellAnchorMetadata.TryGetValue(anchorId, out var metadata))
            {
                metadata = new CellAnchorMetadata(anchorId);
                cellAnchorMetadata.Add(anchorId, metadata);
            }

            metadata.position = position;
            SetDirtyIfNecessary();
        }
        
        public void SetAnchorPositions(Table table)
        {
            foreach (var column in table.OrderedColumns)
            {
                SetAnchorPosition(column.Id, column.Position);
            }

            foreach (var row in table.OrderedRows)
            {
                SetAnchorPosition(row.Id, row.Position);
            }
        }
        
        public void SetAnchorSize(int anchorId, Vector2 size)
        {
            if (!cellAnchorMetadata.TryGetValue(anchorId, out var metadata))
            {
                metadata = new CellAnchorMetadata(anchorId);
                cellAnchorMetadata.Add(anchorId, metadata);
            }

            metadata.size = size;
            SetDirtyIfNecessary();
        }
        
        public void SetItemsType(Type type)
        {
            if (type == null)
            {
                itemsTypeName = null;
                return;
            }
            
            itemsTypeName = type.AssemblyQualifiedName;
            SetDirtyIfNecessary();
        }
        
        
        public void ClearAnchorSizes()
        {
            if (cellAnchorMetadata == null) return;

            foreach (var metadata in cellAnchorMetadata.Values)
            {
                metadata.size = Vector2.zero;
            }
            
            SetDirtyIfNecessary();
        }
        
        #endregion

        #region Utility

        public int UpdateRowsPosition()
        {
            SortedList<int, CellAnchorMetadata> sortedMetadata = new SortedList<int, CellAnchorMetadata>();
            List<int> recentlyAddedRows = new List<int>();
            
            foreach (var guid in ItemGUIDs)
            {
                int rowId = HashCodeUtil.CombineHashes(guid);
                if (!cellAnchorMetadata.TryGetValue(rowId, out var metadata))
                {
                    recentlyAddedRows.Add(rowId);
                    continue;
                }
                
                sortedMetadata.Add(metadata.position, metadata);
            }
            
            int newPosition = 1;
            foreach (var metadata in sortedMetadata.Values)
            {
                metadata.position = newPosition++;
            }
            
            foreach (var rowId in recentlyAddedRows)
            {
               SetAnchorPosition(rowId, newPosition++);
            }

            SetDirtyIfNecessary();
            return newPosition - 1; // Return the last position used
        }
        
        public void RemoveAnchorMetadata(int anchorId)
        {
            if (cellAnchorMetadata.ContainsKey(anchorId))
            {
                cellAnchorMetadata.Remove(anchorId);
            }
            hiddenFields.Remove(anchorId);
            
            SetDirtyIfNecessary();
        }
        
        public void RemoveCellMetadata(int cellId)
        {
            expandedTables.Remove(cellId);
            transposedTables.Remove(cellId);
        }

        public void SwapMetadata(CellAnchor cellAnchor1, CellAnchor cellAnchor2)
        {
            if(cellAnchor1.GetType() != cellAnchor2.GetType())
                return;
            
            if(cellAnchor1 is Row row1 && cellAnchor2 is Row row2)
            {
                SwapMetadata(row1, row2);
            }
            else if(cellAnchor1 is Column column1 && cellAnchor2 is Column column2)
            {
                SwapMetadata(column1, column2);
            }
        }
        
        private void SwapMetadata(Row row1, Row row2)
        {
            if(row1.SerializedObject.SerializedType.Type != row2.SerializedObject.SerializedType.Type)
                return;

            int row1Id = row1.Id;
            int row2Id = row2.Id;
            
            SwapSizes(row1Id, row2Id);
            SwapVisibility(row1Id, row2Id);
            SwapPositions(row1Id, row2Id);
           
            IReadOnlyList<Cell> row1Cells = row1.OrderedCells;
            IReadOnlyList<Cell> row2Cells = row2.OrderedCells;
            
            for (int i = 0; i < row1Cells.Count; i++)
            {
                Cell cell1 = row1Cells[i];
                Cell cell2 = row2Cells[i];
                
                if(cell1 is SubTableCell subTableCell1 && cell2 is SubTableCell subTableCell2)
                {
                    SwapMetadata(subTableCell1, subTableCell2);
                }
                
                SwapFunctions(cell1.Id, cell2.Id);
            }
        }
        
        private void SwapMetadata(Column column1, Column column2)
        {
            int column1Id = column1.Id;
            int column2Id = column2.Id;
            
            SwapSizes(column1Id, column2Id);
            SwapVisibility(column1Id, column2Id);
        }
        
        private void SwapMetadata(SubTableCell subTableCell1, SubTableCell subTableCell2)
        {
            int subTableCell1Id = subTableCell1.Id;
            int subTableCell2Id = subTableCell2.Id;
            
            SwapExpanded(subTableCell1Id, subTableCell2Id);
            
            IReadOnlyList<Column> columns1 = subTableCell1.SubTable.OrderedColumns;
            IReadOnlyList<Column> columns2 = subTableCell2.SubTable.OrderedColumns;
            IReadOnlyList<Row> rows1 = subTableCell1.SubTable.OrderedRows;
            IReadOnlyList<Row> rows2 = subTableCell2.SubTable.OrderedRows;
            
            for (int i = 0; i < columns1.Count; i++)
            {
                Column column1 = columns1[i];
                Column column2 = columns2[i];
                
                SwapMetadata(column1, column2);
            }
            
            for (int i = 0; i < rows1.Count; i++)
            {
                Row row1 = rows1[i];
                Row row2 = rows2[i];
                
                SwapMetadata(row1, row2);
            }
            
            //Swap the corner sizes
            SwapSizes(subTableCell1Id, subTableCell2Id);
        }
        
        private void SwapExpanded(int cell1, int cell2)
        {
            bool cell1Expanded = IsTableExpanded(cell1);
            bool cell2Expanded = IsTableExpanded(cell2);
            
            SetTableExpanded(cell1, cell2Expanded);
            SetTableExpanded(cell2, cell1Expanded);
        }
        
        private void SwapFunctions(int cell1, int cell2)
        {
            string function1 = GetFunction(cell1);
            string function2 = GetFunction(cell2);
            
            SetFunction(cell1, function2);
            SetFunction(cell2, function1);
        }
        
        private void SwapSizes(int anchor1, int anchor2)
        {
            Vector2 anchor1Size = GetAnchorSize(anchor1);
            Vector2 anchor2Size = GetAnchorSize(anchor2);

            SetAnchorSize(anchor1, anchor2Size);
            SetAnchorSize(anchor2, anchor1Size);
        }
        
        private void SwapPositions(int anchor1, int anchor2)
        {
            int anchor1Position = GetAnchorPosition(anchor1);
            int anchor2Position = GetAnchorPosition(anchor2);
            
            SetAnchorPosition(anchor1, anchor2Position);
            SetAnchorPosition(anchor2, anchor1Position);
        }
        
        private void SwapVisibility(int anchor1, int anchor2)
        {
            bool anchor1Visible = IsFieldVisible(anchor1);
            bool anchor2Visible = IsFieldVisible(anchor2);
            
            SetFieldVisible(anchor1, anchor2Visible);
            SetFieldVisible(anchor2, anchor1Visible);
        }
        
        private void RemoveNonExistingItemGUIDs()
        {
            if(IsTypeBound) return;
            
            var itemGUIDsToRemove = new List<string>();
            foreach (var guid in itemGUIDs)
            {
                if (string.IsNullOrEmpty(guid) || string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(guid))
                    || !PathUtil.TryLoadAsset(AssetDatabase.GUIDToAssetPath(guid), out _))
                {
                    itemGUIDsToRemove.Add(guid);
                }
            }

            foreach (var guid in itemGUIDsToRemove)
            {
                itemGUIDs.Remove(guid);
            }
        }

        #endregion
        
        #region Serialization

        private void SetDirtyIfNecessary()
        {
            if (!EditorUtility.IsDirty(this))
                EditorUtility.SetDirty(this);
        }
        
        #endregion

        public static TableMetadata Clone(TableMetadata oldTableMetadata)
        {
            if (oldTableMetadata == null) return null;

            TableMetadata newTableMetadata = CreateInstance<TableMetadata>();
            Copy(newTableMetadata, oldTableMetadata);
            
            return newTableMetadata;
        }

        public static void Copy(TableMetadata to, TableMetadata from)
        {
            if (to == null || from == null) return;

            to.name = from.name;
            to.itemsTypeName = from.itemsTypeName;
            to.bindingTypeName = from.bindingTypeName;
            to.itemGUIDs = new SerializedHashSet<string>(from.itemGUIDs);
            to.cellAnchorMetadata = new SerializedDictionary<int, CellAnchorMetadata>();
            foreach (var kvp in from.cellAnchorMetadata)
            {
                to.cellAnchorMetadata.Add(kvp.Key, new CellAnchorMetadata(kvp.Key)
                {
                    position = kvp.Value.position,
                    size = kvp.Value.size
                });
            }
            
            to.expandedTables = new SerializedHashSet<int>(from.expandedTables);
            to.transposedTables = new SerializedHashSet<int>(from.transposedTables);
            to.hiddenFields = new SerializedHashSet<int>(from.hiddenFields);

            to.SetDirtyIfNecessary();
        }

    }

    [Serializable]
    internal class CellAnchorMetadata
    {
        public CellAnchorMetadata(int id)
        {
            this.id = id;
        }
        
        public readonly int id;
        public int position;
        public Vector2 size;
    }
}