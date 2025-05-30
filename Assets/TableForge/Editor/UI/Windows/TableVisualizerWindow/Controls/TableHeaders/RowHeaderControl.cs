using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class RowHeaderControl : HeaderControl
    {
        private static ObjectPool<RowHeaderControl> _pool = new(() => new RowHeaderControl());
        private static ObjectPool<RowControl> _rowControlPool = new(() => new RowControl());
        
        private bool _isChangingName;
        private readonly Label _headerLabel;
        private readonly TextField _textField;
        public RowControl RowControl { get; set; }
        
        public static RowHeaderControl GetPooled(CellAnchor cellAnchor, TableControl tableControl)
        {
            var control = _pool.Get();
            control.OnEnable(cellAnchor, tableControl);
            return control;
        }
        
        private RowHeaderControl()
        {
            AddToClassList(USSClasses.TableHeaderCellVertical);
            
            _headerLabel = new Label();
            _headerLabel.AddToClassList(USSClasses.TableHeaderText);
            _textField = new TextField();
            
            _textField.RegisterCallback<FocusOutEvent>(evt =>
            {
                _isChangingName = false;
                TryChangeName();
            });
            
            Add(_headerLabel);
        }

        protected override void OnEnable(CellAnchor cellAnchor, TableControl tableControl)
        {
            base.OnEnable(cellAnchor, tableControl);
            if(tableControl.Parent != null)
            {
                AddToClassList(USSClasses.SubTableHeaderCellVertical);
            }
            
            var title = NameResolver.ResolveHeaderStyledName(cellAnchor, tableControl.TableAttributes.RowHeaderVisibility);
            _headerLabel.text = title;
            if(tableControl.Parent != null)
            {
                _headerLabel.AddToClassList(USSClasses.SubTableHeaderText);
            }
            
            _textField.value = cellAnchor.Name;
            
            TableControl.VerticalResizer.HandleResize(this);
            TableControl.HeaderSwapper.HandleSwapping(this);
            
            RowControl = _rowControlPool.Get();
            RowControl.Initialize(cellAnchor, tableControl);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            TableControl.VerticalResizer.Dispose(this);
            TableControl.HeaderSwapper.Dispose(this);
            RowControl.ClearRow();

            _rowControlPool.Release(RowControl);
            _pool.Release(this);
        }

        protected override void SelectionChanged()
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
            RefreshName();
        }

        private void RefreshName()
        {
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
                
                _textField.value = CellAnchor.Name;
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
                        HideTextField();
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
            string path = AssetDatabase.GetAssetPath(((Row)CellAnchor).SerializedObject.RootObject);
            AssetUtils.RenameAsset(path, _textField.value.Trim());
            HideTextField();
        }

        private void HideTextField()
        {
            RefreshName();
            Remove(_textField);
            Add(_headerLabel);
            
            //Recover focus on the window in case we lost it
            schedule.Execute(() =>
            {
                TableControl.Root.Focus();
            }).ExecuteLater(0);
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