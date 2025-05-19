using System.Linq;
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
            _textField.RegisterCallback<FocusOutEvent>(evt =>
            {
                _isChangingName = false;
                TryChangeName();
            });
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
                AssetUtils.DeleteAsset(((Row)CellAnchor).SerializedObject.RootObjectGuid, RemoveThisRow);
            });
            
            obj.menu.AppendSeparator();

            if (TableControl.CellSelector.GetSelectedRows().Count > 1)
            {
                if (!TableControl.Metadata.IsTypeBound)
                    obj.menu.AppendAction("Remove selected items", (_) => RemoveSelectedRows());
                obj.menu.AppendAction("Delete associated assets", (_) =>
                {
                    AssetUtils.DeleteAssets(TableControl.CellSelector.GetSelectedRows().Select(x => x.SerializedObject.RootObjectGuid), RemoveSelectedRows);
                });
            }
        }

        private void TryChangeName()
        {
            string oldPath = AssetDatabase.GetAssetPath(((Row)CellAnchor).SerializedObject.RootObject);
            string newName = AssetUtils.RenameAsset(oldPath, _textField.value.Trim());
            
            if (newName == _name)
            {
                _textField.value = _name;
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