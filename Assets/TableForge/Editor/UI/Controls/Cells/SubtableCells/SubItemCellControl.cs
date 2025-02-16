using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class SubItemCellControl : SubTableCellControl
    {
        private TableControl _subTableControl;
        public SubItemCellControl(SubItemCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            _subTableControl = new TableControl(tableControl.Root);
            _subTableControl.SetTable(cell.SubTable);
            
            VisualElement container = new VisualElement();
            container.style.marginBottom = 5;
            container.style.marginTop = 5;
            container.style.marginLeft = 5;
            container.style.marginRight = 5;
            container.style.borderBottomWidth = 1;
            container.style.borderTopWidth = 1;
            container.style.borderLeftWidth = 1;
            container.style.borderRightWidth = 1;
            container.style.borderBottomColor = new StyleColor(Color.black);
            container.style.borderTopColor = new StyleColor(Color.black);
            container.style.borderLeftColor = new StyleColor(Color.black);
            container.style.borderRightColor = new StyleColor(Color.black);
            container.style.borderBottomRightRadius = 5;
            container.style.borderTopRightRadius = 5;
            container.style.borderBottomLeftRadius = 5;
            container.style.borderTopLeftRadius = 5;
            container.Add(_subTableControl);
            Add(container);
            
            InitializeSize();
            IsSelected = false;
        }

        protected override void InitializeSize()
        {
            float width = 10, height = 10;
            foreach (var column in _subTableControl.ColumnData.Values)
            {
                width += column.PreferredWidth + 0.5f;
            }
            
            foreach (var row in _subTableControl.RowData.Values)
            {
                height += row.PreferredHeight + 0.5f;
            }
            
            SetDesiredSize(width, height);
        }
    }
}