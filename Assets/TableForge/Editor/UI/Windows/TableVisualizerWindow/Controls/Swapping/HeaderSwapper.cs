using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    internal class HeaderSwapper
    {
        private readonly TableControl _tableControl;
        private readonly Dictionary<HeaderControl, SwappingDragger> _swappingDraggers = new();
        
        public HeaderSwapper(TableControl tableControl)
        {
            _tableControl = tableControl;
        }
        
        public void HandleSwapping(HeaderControl headerControl)
        {
            if(headerControl is RowHeaderControl && headerControl.CellAnchor is Row
               && _tableControl.TableAttributes.rowReorderMode != TableReorderMode.None
               && _swappingDraggers.TryAdd(headerControl, new RowSwappingDragger(_tableControl)))
            {
                headerControl.AddManipulator(_swappingDraggers[headerControl]);
            }
        }
        
        public void Dispose(HeaderControl headerControl)
        {
            if (headerControl == null) return;
            
            if(_swappingDraggers.TryGetValue(headerControl, out var dragger))
            {
                dragger.Dispose();
                _swappingDraggers.Remove(headerControl);
            }
        }
    }
}