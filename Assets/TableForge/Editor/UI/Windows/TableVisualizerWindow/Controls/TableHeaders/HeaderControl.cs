using UnityEditor;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal abstract class HeaderControl : VisualElement
    {
        public readonly TableControl TableControl;
        private bool _isSelected;
        private bool _isSubSelected;
        
        public CellAnchor CellAnchor { get; protected set; }

        public bool IsSubSelected
        {
            get => _isSubSelected;
            set
            {
                if (!value || _isSelected)
                    RemoveFromClassList(USSClasses.SubSelectedHeader);
                else
                    AddToClassList(USSClasses.SubSelectedHeader);

                _isSubSelected = value;
            }
        }
        
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (!value)
                    RemoveFromClassList(USSClasses.SelectedHeader);
                else
                    AddToClassList(USSClasses.SelectedHeader);

                _isSelected = value;
            }
        }
        public bool IsVisible { get; set; }
        public string Name => CellAnchor?.Name ?? string.Empty;
        public int Id => CellAnchor?.Id ?? 0;


        protected HeaderControl(CellAnchor cellAnchor, TableControl tableControl)
        {
            CellAnchor = cellAnchor;
            TableControl = tableControl;
            
            IsSelected = tableControl.CellSelector.IsAnchorSelected(cellAnchor);
            IsSubSelected = tableControl.CellSelector.IsAnchorSubSelected(cellAnchor);
            tableControl.CellSelector.OnSelectionChanged += () =>
            {
                IsSelected = tableControl.CellSelector.IsAnchorSelected(cellAnchor);
                IsSubSelected = tableControl.CellSelector.IsAnchorSubSelected(cellAnchor);
            };

            if (cellAnchor is Row && !tableControl.Metadata.IsTypeBound && tableControl.Parent == null)
            {
                this.AddManipulator(new ContextualMenuManipulator(MenuBuilder));
            }
        }

        private void MenuBuilder(ContextualMenuPopulateEvent obj)
        {
            obj.menu.AppendAction("Remove this item", (_) => RemoveThisRow());
            obj.menu.AppendAction("Delete associated asset", (_) =>
            {
                bool confirmed = EditorUtility.DisplayDialog(
                    "Confirm Action",
                    "Are you sure you want to delete the selected asset? This action cannot be undone.",
                    "Yes",
                    "No"
                );

                if (confirmed)
                {
                    RemoveThisRow();
                    string path = AssetDatabase.GUIDToAssetPath(((Row)CellAnchor).SerializedObject.RootObjectGuid);
                    AssetDatabase.DeleteAsset(path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            });
            
            obj.menu.AppendSeparator();
            
            obj.menu.AppendAction("Remove selected items", (_) => RemoveSelectedRows());
            obj.menu.AppendAction("Delete associated assets", (_) =>
            {
                bool confirmed = EditorUtility.DisplayDialog(
                    "Confirm Action",
                    "Are you sure you want to delete the selected assets? This action cannot be undone. (multiple assets selected)",
                    "Yes",
                    "No"
                );

                if (confirmed)
                {
                    var selectedRows = TableControl.CellSelector.GetSelectedRows();
                    RemoveSelectedRows();
                    foreach (var row in  selectedRows)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(row.SerializedObject.RootObjectGuid);
                        AssetDatabase.DeleteAsset(path);
                    }
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            });
        }
        
        private void RemoveThisRow()
        {
            TableControl.RemoveRow(CellAnchor.Id);
            TableControl.RebuildPage();
        }

        private void RemoveSelectedRows()
        {
            var selectedRows = TableControl.CellSelector.GetSelectedRows();
            selectedRows.Sort((a, b) => b.Position.CompareTo(a.Position));

            foreach (var selected in selectedRows)
            {
                if (selected.Table != TableControl.TableData) continue;
                TableControl.RemoveRow(selected.Id);
            }

            TableControl.RebuildPage();
        }
    }
}