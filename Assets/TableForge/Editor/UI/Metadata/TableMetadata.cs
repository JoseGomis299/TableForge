using System;
using UnityEditor;
using UnityEngine;

namespace TableForge.UI
{
    internal class TableMetadata : ScriptableObject
    {
        #region Fields

        [SerializeField, HideInInspector] private string tableName;
        
        [SerializeField, HideInInspector] private SerializedHashSet<int> expandedTables = new();
        [SerializeField, HideInInspector] private SerializedHashSet<int> invertedTables = new();
        [SerializeField, HideInInspector] private SerializedHashSet<int> hiddenFields = new();
        
        [SerializeField, HideInInspector] private SerializedDictionary<int, CellAnchorMetadata> cellAnchorMetadata = new();

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
        
        public bool IsInverted
        {
            get => invertedTables.Contains(0);
            set
            {
                if (value) invertedTables.Add(0);
                else invertedTables.Remove(0);
                
                SetDirtyIfNecessary();
            }
        }
        
        #endregion
        
        #region Getters
        
        public bool IsFieldVisible(int anchorId)
        {
            return !hiddenFields.Contains(anchorId);
        }
        
        public bool IsTableExpanded(int subTableCellId)
        {
            return expandedTables.Contains(subTableCellId);
        }
        
        public bool IsTableInverted(int subTableCellId)
        {
            return invertedTables.Contains(subTableCellId);
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
        
        
        #endregion
        
        #region Setters
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
        
        public void SetTableInverted(int subTableCellId, bool isInverted)
        {
            if(isInverted) invertedTables.Add(subTableCellId);
            else invertedTables.Remove(subTableCellId);
            
            SetDirtyIfNecessary();
        }
        
        public void SetAnchorPosition(int anchorId, int position)
        {
            if (!cellAnchorMetadata.TryGetValue(anchorId, out var metadata))
            {
                metadata = new CellAnchorMetadata {id = anchorId};
                cellAnchorMetadata.Add(anchorId, metadata);
            }

            metadata.position = position;
            SetDirtyIfNecessary();
        }
        
        public void SetAnchorSize(int anchorId, Vector2 size)
        {
            if (!cellAnchorMetadata.TryGetValue(anchorId, out var metadata))
            {
                metadata = new CellAnchorMetadata {id = anchorId};
                cellAnchorMetadata.Add(anchorId, metadata);
            }

            metadata.size = size;
            SetDirtyIfNecessary();
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
        public int id;
        public int position;
        public Vector2 size;
    }
    
}