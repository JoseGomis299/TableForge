using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TableForge.UI
{
    internal class TableMetadata : ScriptableObject
    {
        #region Fields

        [SerializeField, HideInInspector] private string tableName;
        
        [SerializeField, HideInInspector] private SerializedHashSet<string> expandedTables = new();
        [SerializeField, HideInInspector] private SerializedHashSet<string> transposedTables = new();
        [SerializeField, HideInInspector] private SerializedHashSet<string> hiddenFields = new();
        
        [SerializeField] private SerializedDictionary<string, CellAnchorMetadata> cellAnchorMetadata = new();

        #endregion

        #region Properties

        public string Name
        {
            get => tableName;
            set
            {
                name = value;
                tableName = value;
                
                SetDirtyIfNecessary();
            }
        }
        
        public bool IsTransposed
        {
            get => transposedTables.Contains("");
            set
            {
                if (value) transposedTables.Add("");
                else transposedTables.Remove("");
                
                SetDirtyIfNecessary();
            }
        }
        
        #endregion
        
        #region Getters
        
        public bool IsFieldVisible(string anchorId)
        {
            return !hiddenFields.Contains(anchorId);
        }
        
        public bool IsTableExpanded(string subTableCellId)
        {
            return expandedTables.Contains(subTableCellId);
        }
        
        public int GetAnchorPosition(string anchorId)
        {
            return cellAnchorMetadata.TryGetValue(anchorId, out var metadata) ? metadata.position : 0;
        }
        
        public Vector2 GetAnchorSize(string anchorId)
        {
            cellAnchorMetadata ??= new SerializedDictionary<string, CellAnchorMetadata>();
            return cellAnchorMetadata.TryGetValue(anchorId, out var metadata) ? metadata.size : Vector2.zero;
        }
        
        
        #endregion
        
        #region Setters
        public void SetFieldVisible(string anchorId, bool isVisible)
        {
            if(isVisible) hiddenFields.Remove(anchorId);
            else hiddenFields.Add(anchorId);
            
            SetDirtyIfNecessary();
        }
        
        public void SetTableExpanded(string subTableCellId, bool isExpanded)
        {
            if(isExpanded) expandedTables.Add(subTableCellId);
            else expandedTables.Remove(subTableCellId);
            
            SetDirtyIfNecessary();
        }
        
        public void SetAnchorPosition(string anchorId, int position)
        {
            if (!cellAnchorMetadata.TryGetValue(anchorId, out var metadata))
            {
                metadata = new CellAnchorMetadata();
                cellAnchorMetadata.Add(anchorId, metadata);
            }

            metadata.position = position;
            SetDirtyIfNecessary();
        }
        
        public void SetAnchorSize(string anchorId, Vector2 size)
        {
            if (!cellAnchorMetadata.TryGetValue(anchorId, out var metadata))
            {
                metadata = new CellAnchorMetadata();
                cellAnchorMetadata.Add(anchorId, metadata);
            }

            metadata.size = size;
            SetDirtyIfNecessary();
        }
        
        #endregion

        #region Utility

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

            string row1Id = row1.Id;
            string row2Id = row2.Id;
            
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
            }
        }
        
        private void SwapMetadata(Column column1, Column column2)
        {
            string column1Id = column1.Id;
            string column2Id = column2.Id;
            
            SwapSizes(column1Id, column2Id);
            SwapVisibility(column1Id, column2Id);
        }
        
        private void SwapMetadata(SubTableCell subTableCell1, SubTableCell subTableCell2)
        {
            string subTableCell1Id = subTableCell1.Id;
            string subTableCell2Id = subTableCell2.Id;
            
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
        
        private void SwapExpanded(string cell1, string cell2)
        {
            bool cell1Expanded = IsTableExpanded(cell1);
            bool cell2Expanded = IsTableExpanded(cell2);
            
            SetTableExpanded(cell1, cell2Expanded);
            SetTableExpanded(cell2, cell1Expanded);
        }
        
        private void SwapSizes(string anchor1, string anchor2)
        {
            Vector2 anchor1Size = GetAnchorSize(anchor1);
            Vector2 anchor2Size = GetAnchorSize(anchor2);

            SetAnchorSize(anchor1, anchor2Size);
            SetAnchorSize(anchor2, anchor1Size);
        }
        
        private void SwapPositions(string anchor1, string anchor2)
        {
            int anchor1Position = GetAnchorPosition(anchor1);
            int anchor2Position = GetAnchorPosition(anchor2);
            
            SetAnchorPosition(anchor1, anchor2Position);
            SetAnchorPosition(anchor2, anchor1Position);
        }
        
        private void SwapVisibility(string anchor1, string anchor2)
        {
            bool anchor1Visible = IsFieldVisible(anchor1);
            bool anchor2Visible = IsFieldVisible(anchor2);
            
            SetFieldVisible(anchor1, anchor2Visible);
            SetFieldVisible(anchor2, anchor1Visible);
        }

        #endregion
        
        #region Serialization

        private void SetDirtyIfNecessary()
        {
            if (!EditorUtility.IsDirty(this))
                EditorUtility.SetDirty(this);
        }
        
        #endregion
    }

    [Serializable]
    internal class CellAnchorMetadata
    {
        public int position;
        public Vector2 size;
    }
    
}