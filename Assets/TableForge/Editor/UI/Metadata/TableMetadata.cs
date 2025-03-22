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

        public string Name
        {
            get => tableName;
            set
            {
                name = value;
                tableName = value;
            }
        }

        public bool IsInverted { get => isInverted; set => isInverted = value; }
        public List<int> VisibleFields { get => visibleFields; set => visibleFields = value; }

        public SerializedDictionary<int, CellAnchorMetadata> RowMetadata { get => rowMetadata; set => rowMetadata = value; }
        public SerializedDictionary<int, CellAnchorMetadata> ColumnMetadata { get => columnMetadata; set => columnMetadata = value; }
        public SerializedDictionary<int, CellMetadata> CellMetadata { get => cellMetadata; set => cellMetadata = value; }

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