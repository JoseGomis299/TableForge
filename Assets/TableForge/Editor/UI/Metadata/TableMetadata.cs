using System;
using System.Collections.Generic;
using UnityEngine;

namespace TableForge.UI
{
    public class TableMetadata : ScriptableObject
    {
        #region Fields

        [SerializeField, HideInInspector] private string tableName;
        [SerializeField, HideInInspector] private bool isInverted;
        [SerializeField, HideInInspector] private List<int> visibleFields;

        [SerializeField, HideInInspector] private SerializedDictionary<int, CellAnchorMetadata> rowMetadata;

        [SerializeField, HideInInspector] private SerializedDictionary<int, CellAnchorMetadata> columnMetadata;

        [SerializeField, HideInInspector] private SerializedDictionary<int, CellMetadata> cellMetadata;

        #endregion

        #region Properties

        public string Name => tableName;
        public bool IsInverted => isInverted;
        public List<int> VisibleFields => visibleFields;

        public Dictionary<int, CellAnchorMetadata> RowMetadata => rowMetadata;
        public Dictionary<int, CellAnchorMetadata> ColumnMetadata => columnMetadata;
        public Dictionary<int, CellMetadata> CellMetadata => cellMetadata;

        #endregion
    }

    [Serializable]
    public class CellAnchorMetadata
    {
        public int id;
        public int position;
        public Vector2 size;
    }
    
    [Serializable]
    public class CellMetadata
    {
        public int id;
        public bool isExpanded;
    }
}