using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    internal class ColumnHeaderControl : HeaderControl
    {
        private static readonly ObjectPool<ColumnHeaderControl> _pool = new(() => new ColumnHeaderControl());
        
        private readonly Label _headerLabel;
        
        public static ColumnHeaderControl GetPooled(CellAnchor cellAnchor, TableControl tableControl)
        {
            var control = _pool.Get();
            control.OnEnable(cellAnchor, tableControl);
            return control;
        }
        
        private ColumnHeaderControl()
        {
            AddToClassList(USSClasses.TableHeaderCellHorizontal);
            _headerLabel = new Label();
        }

        protected override void OnEnable(CellAnchor cellAnchor, TableControl tableControl)
        {
            base.OnEnable(cellAnchor, tableControl);
            if (!tableControl.Filterer.IsVisible(CellAnchor.GetRootAnchor().Id))
            {
                style.display = DisplayStyle.None;
                return;
            }
            
            style.display = DisplayStyle.Flex;
            string title = NameResolver.ResolveHeaderStyledName(cellAnchor, tableControl.TableAttributes.columnHeaderVisibility);
            _headerLabel.text = title;
            _headerLabel.AddToClassList(USSClasses.TableHeaderText);
            if(tableControl.Parent != null)
            {
                _headerLabel.AddToClassList(USSClasses.SubTableHeaderText);
            }
            Add(_headerLabel);

            TableControl.HorizontalResizer.HandleResize(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            style.width = 0;
            TableControl.HorizontalResizer.Dispose(this);
            _pool.Release(this);
        }

        protected override void BuildContextualMenu(ContextualMenuPopulateEvent obj)
        {
            ExpandCollapseBuilder(obj);
            obj.menu.AppendSeparator();
            SortColumnBuilder(obj);
        }
    }
}