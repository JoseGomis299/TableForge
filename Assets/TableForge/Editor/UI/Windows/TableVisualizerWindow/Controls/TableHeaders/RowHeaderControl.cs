using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class RowHeaderControl : HeaderControl
    {
        private bool _isChangingName;
        private readonly Label _headerLabel;
        private readonly TextField _textField;
        private string _name;
        public RowControl RowControl { get; set; }
        
        public RowHeaderControl(CellAnchor cellAnchor, TableControl tableControl) : base(cellAnchor, tableControl)
        {
            AddToClassList(USSClasses.TableHeaderCellVertical);
            if(tableControl.Parent != null)
            {
                AddToClassList(USSClasses.SubTableHeaderCellVertical);
            }
            
            var title = NameResolver.ResolveHeaderStyledName(cellAnchor, tableControl.TableAttributes.RowHeaderVisibility);
            _headerLabel = new Label(title);
            _headerLabel.AddToClassList(USSClasses.TableHeaderText);
            if(tableControl.Parent != null)
            {
                _headerLabel.AddToClassList(USSClasses.SubTableHeaderText);
            }           
            Add(_headerLabel);
            
            _name = NameResolver.ResolveHeaderName(cellAnchor, tableControl.TableAttributes.RowHeaderVisibility);
            _textField = new TextField { value = _name };
            
            TableControl.VerticalResizer.HandleResize(this);
            
            OnSelectionChanged += SelectionChanged;
        }

        private void SelectionChanged()
        {
            if(!IsSelected && _isChangingName)
            {
                _isChangingName = false;
                TryChangeName();
            }
        }

        public void Refresh()
        {
            RowControl.Refresh();
            _headerLabel.text = NameResolver.ResolveHeaderStyledName(CellAnchor, TableControl.TableAttributes.RowHeaderVisibility);
        }

        protected override void BuildContextualMenu(ContextualMenuPopulateEvent obj)
        {
            if (_isChangingName) return;
            
            obj.menu.AppendAction("Focus asset in Inspector", (_) =>
            {
                var targetObject = ((Row)CellAnchor).SerializedObject.RootObject;
                Selection.activeObject = targetObject;
                EditorGUIUtility.PingObject(targetObject);
            });
            
            obj.menu.AppendAction("Rename asset", (_) =>
            {
                _isChangingName = true;
                
                _textField.value = _name;
                _textField.AddToClassList(USSClasses.TableHeaderText);
                _textField.RegisterCallback<KeyDownEvent>((keyEvt) =>
                {
                    if (keyEvt.keyCode is KeyCode.Return or KeyCode.KeypadEnter)
                    {
                         TryChangeName();
                        _isChangingName = false;
                    }
                    else if (keyEvt.keyCode == KeyCode.Escape)
                    {
                        HideTextField(_name);
                        _isChangingName = false;
                    }
                });

                Remove(_headerLabel);
                Add(_textField);
                _textField.Focus();
                _textField.SelectAll();
            });

            
            obj.menu.AppendSeparator();
            
            ExpandCollapseBuilder(obj);
            
            obj.menu.AppendSeparator();
            
            if(!TableControl.Metadata.IsTypeBound)
                obj.menu.AppendAction("Remove this item", (_) => RemoveThisRow());
            obj.menu.AppendAction("Delete this asset", (_) =>
            {
                bool confirmed = EditorUtility.DisplayDialog(
                    "Confirm Action",
                    "Are you sure you want to delete the selected asset? This action cannot be undone."  + " (\""+ CellAnchor.Name +"\")",
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

            if (TableControl.CellSelector.GetSelectedRows().Count > 1)
            {
                if (!TableControl.Metadata.IsTypeBound)
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
                        foreach (var row in selectedRows)
                        {
                            string path = AssetDatabase.GUIDToAssetPath(row.SerializedObject.RootObjectGuid);
                            AssetDatabase.DeleteAsset(path);
                        }

                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                });
            }
        }

        private void TryChangeName()
        {
            string oldPath = AssetDatabase.GetAssetPath(((Row)CellAnchor).SerializedObject.RootObject);

            string oldName = _name;
            string directory = oldPath.Substring(0, oldPath.LastIndexOf('/'));
            string extension = oldPath.Substring(oldPath.LastIndexOf('.'));
            
            string baseName = _textField.value.Trim();
            string newName = baseName;
            string newPath = $"{directory}/{newName}{extension}";
            int counter = 1;

            while (AssetDatabase.AssetPathExists(newPath))
            {
                newName = $"{baseName} {counter++}";
                newPath = $"{directory}/{newName}{extension}";
            }

            string error = AssetDatabase.RenameAsset(oldPath, newName);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"Failed to rename asset: {error}");
                _textField.value = oldName;
                newName = oldName;
            }
            else
            {
                _headerLabel.text = newName;
            }

            HideTextField(newName);
        }

        private void HideTextField(string newName)
        {
            Remove(_textField);
            _headerLabel.text = newName;
            _name = newName;
            Add(_headerLabel);
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