using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class RowHeaderControl : HeaderControl
    {
        public RowControl RowControl { get; set; }

        private readonly Label _headerLabel;

        public RowHeaderControl(CellAnchor cellAnchor, TableControl tableControl) : base(cellAnchor, tableControl)
        {
            AddToClassList(USSClasses.TableHeaderCellVertical);
            if(tableControl.Parent != null)
            {
                AddToClassList(USSClasses.SubTableHeaderCellVertical);
            }
            
            string title = NameResolver.ResolveHeaderStyledName(cellAnchor, tableControl.TableAttributes.RowHeaderVisibility);
            _headerLabel = new Label(title);
            _headerLabel.AddToClassList(USSClasses.TableHeaderText);
            if(tableControl.Parent != null)
            {
                _headerLabel.AddToClassList(USSClasses.SubTableHeaderText);
            }           
            Add(_headerLabel);

            TableControl.VerticalResizer.HandleResize(this);
        }
        
        public void Refresh()
        {
            RowControl.Refresh();
            _headerLabel.text = NameResolver.ResolveHeaderStyledName(CellAnchor, TableControl.TableAttributes.RowHeaderVisibility);
        }
    }
}