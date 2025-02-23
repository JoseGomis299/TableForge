using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class RowSwappingDragger : SwappingDragger
    {
        public RowSwappingDragger(TableControl tableControl) : base(tableControl)
        {
        }
        
        protected override void CacheHeaderBounds()
        {
            HeaderBounds.Clear();
            foreach (var rowHeader in TableControl.RowHeaders.Values)
            {
                HeaderBounds.Add(rowHeader.Id, rowHeader.worldBound);
            }
        }

        protected override void MoveElements(MouseMoveEvent e)
        {
            if (target is not RowHeaderControl rowHeaderControl) return;
            Vector3 delta = (Vector3) e.mouseDelta - new Vector3(e.mouseDelta.x, 0);
            
            target.transform.position += delta;
            rowHeaderControl.RowControl.transform.position += delta;

            foreach (var idBoundPar in HeaderBounds)
            {
                int id = idBoundPar.Key;
                Rect rowBound = idBoundPar.Value;
                RowHeaderControl rowHeader = TableControl.RowHeaders[id];
                Vector3 midPoint = (Vector3)target.worldBound.position + (Vector3)target.worldBound.size / 2;
                    
                if(rowBound.Contains(midPoint))
                {
                    LastHeaderId = rowHeader.Id;
                }

                if (rowHeader == target || !rowHeader.worldBound.Contains(midPoint)) continue;
                    
                Vector3 positionBeforeMoving = rowHeader.worldBound.position;
                Vector3 newRowPosition = FinalPosition - (Vector3)rowHeader.worldBound.position;
                rowHeader.transform.position += newRowPosition;
                rowHeader.RowControl.transform.position += newRowPosition;
                FinalPosition = positionBeforeMoving;
                break;
            }
        }

        protected override void PerformSwap()
        {
            if (target is not RowHeaderControl rowHeaderControl) return;
            foreach (var rowHeader in TableControl.RowHeaders.Values)
            {
                rowHeader.transform.position = Vector3.zero;
                rowHeader.RowControl.transform.position = Vector3.zero;
            }
                
            if (LastHeaderId != 0)
            {
                int rowStartPos = TableControl.RowData[rowHeaderControl.Id].Position;
                int rowEndPos = TableControl.RowData[LastHeaderId].Position;
                    
                TableControl.MoveRow(rowStartPos, rowEndPos);
            }
        }
    }
}