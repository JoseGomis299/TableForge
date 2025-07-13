using UnityEngine;

namespace TableForge.Editor.UI
{
    internal class TableSettingsData : ScriptableObject
    {
        // Data refresh settings
        [HideInInspector] public float pollingInterval = 0.5f;
        [HideInInspector] public bool enablePolling = false;
       
        // Function handling settings
        [HideInInspector] public bool removeFormulaOnCellValueChange = false;
       
        // Header naming settings
        [HideInInspector] public TableHeaderVisibility rowHeaderVisibility = TableHeaderVisibility.ShowHeaderNumberAndName;
        [HideInInspector] public TableHeaderVisibility columnHeaderVisibility = TableHeaderVisibility.ShowHeaderLetterAndName;
    }
}