using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    internal class RowSwappingDragger : SwappingDragger
    {
        private readonly List<RowHeaderControl> _orderedHeaders = new();
        private int _lastHeaderIndex;
        
        public RowSwappingDragger(TableControl tableControl) : base(tableControl)
        {
        }
        
        protected override void OnClick()
        {
            _orderedHeaders.Clear();
            foreach (var rowHeader in tableControl.RowHeaders.Values)
            {
                _orderedHeaders.Add(rowHeader);
            }
            
            _orderedHeaders.Sort((a, b) => tableControl.RowData[a.Id].Position.CompareTo(tableControl.RowData[b.Id].Position));
            if(target is RowHeaderControl rowHeaderControl) 
                tableControl.RowVisibilityManager.LockHeaderVisibility(rowHeaderControl, this);
            
            _lastHeaderIndex = -1;
        }

        protected override void OnRelease()
        {
            if(target is not RowHeaderControl rowHeaderControl) return;
            tableControl.RowVisibilityManager.UnlockHeaderVisibility(rowHeaderControl, this);
        }

        protected override void MoveElements(MouseMoveEvent e)
        {
            Vector3 delta = (Vector3) e.mousePosition - new Vector3(e.mousePosition.x, target.worldBound.y + target.worldBound.size.y / 2);
            MoveElements(delta);
        }

        private void MoveElements(Vector3 delta)
        {
            if (target is not RowHeaderControl rowHeaderControl) return;
            float direction = delta.y > 0 ? -1 : 1;
            
            target.transform.position += delta;
            rowHeaderControl.RowControl.transform.position += delta;
            
            int movingIndex = tableControl.RowData[rowHeaderControl.Id].Position - 1;
            if(_lastHeaderIndex == -1)
                _lastHeaderIndex = movingIndex;
            
            for (var i = 0; i < _orderedHeaders.Count; i++)
            {
                var rowHeader = _orderedHeaders[i];
                Vector3 midPoint = (Vector3)target.worldBound.position + (Vector3)target.worldBound.size / 2;

                if (rowHeader == target || !rowHeader.worldBound.Contains(midPoint)) continue;

                var targetPos = GetPosition(direction, movingIndex, i);
                rowHeader.transform.position = targetPos;
                rowHeader.RowControl.transform.position = targetPos;
                
                //If the header is moved more than one position, ensure that the other headers are in the correct position
                int indexDifference = _lastHeaderIndex - i;
                if (indexDifference * indexDifference > 1)
                {
                    for (int j = 0; j < _orderedHeaders.Count; j++)
                    {
                        if ((i <= j && j < movingIndex) || (i >= j && j > movingIndex))
                        {
                            _orderedHeaders[j].transform.position = targetPos;
                            _orderedHeaders[j].RowControl.transform.position = targetPos;
                        }
                        else
                        {
                            _orderedHeaders[j].transform.position = Vector3.zero;
                            _orderedHeaders[j].RowControl.transform.position = Vector3.zero;
                        }
                    }
                }
                
                _lastHeaderIndex = i;
                break;
            }
            
            bool hasMovedUp = movingIndex > 0 && _orderedHeaders[movingIndex - 1].transform.position != Vector3.zero;
            bool hasMovedDown = movingIndex < _orderedHeaders.Count - 1 && _orderedHeaders[movingIndex + 1].transform.position != Vector3.zero;
            if(!hasMovedUp && !hasMovedDown) 
                _lastHeaderIndex = -1;
        }

        private Vector3 GetPosition(float direction, int movingIndex, int i)
        {
            float targetY = target.worldBound.height * direction;
            float maxY = movingIndex > i ? targetY : 0;
            float minY = movingIndex < i ? targetY : 0;
            Vector3 targetPos =  Mathf.Clamp(targetY, minY, maxY) * Vector3.up;
            return targetPos;
        }

        protected override void PerformSwap()
        {
            if (target is not RowHeaderControl rowHeaderControl) return;
            foreach (var rowHeader in tableControl.RowHeaders.Values)
            {
                rowHeader.transform.position = Vector3.zero;
                rowHeader.RowControl.transform.position = Vector3.zero;
            }
                
            if (_lastHeaderIndex != -1)
            {
                int rowStartPos = tableControl.RowData[rowHeaderControl.Id].Position;
                int rowEndPos = _lastHeaderIndex + 1;
                    
                ReorderHeaderCommand command = new ReorderHeaderCommand(rowStartPos, rowEndPos, tableControl.MoveRow, tableControl.RowData[rowHeaderControl.Id]);
                UndoRedoManager.Do(command);
            }
        }
    }
}