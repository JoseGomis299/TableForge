using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class CellSelectorInputManager
    {
        #region Fields

        private readonly CellSelector _selector;
        private readonly TableControl _tableControl;

        private Vector3 _lastMousePosition;

        #endregion
       
        #region Constructor
        
        public CellSelectorInputManager(CellSelector selector)
        {
            _selector = selector;
            _tableControl = _selector.TableControl;
            RegisterCallbacks();
        }
        
        #endregion
        
        #region Callback Registration
        
        private void RegisterCallbacks()
        {
            VisualElement content = _tableControl.ScrollView.contentContainer;
            content.RegisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.TrickleDown);
            content.RegisterCallback<MouseDownEvent>(OnMouseDown_NoTrickle, TrickleDown.NoTrickleDown);
            content.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            _tableControl.Root.RegisterCallback<KeyDownEvent>(OnKeyDown);
        }
        
        #endregion

        #region Mouse Event Handlers

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (!IsValidClick(evt))
                return;
            
            _lastMousePosition = evt.mousePosition;
            _selector.PreselectCells(_lastMousePosition, evt.ctrlKey, evt.shiftKey);
        }

        private void OnMouseDown_NoTrickle(MouseDownEvent evt)
        {
            if (!IsValidClick(evt))
                return;
            
            _selector.ConfirmSelection();
            evt.StopPropagation();
        }
        
        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (evt.pressedButtons != 1 || !IsValidClick(evt))
                return;

            float distance = Vector3.Distance(evt.mousePosition, _lastMousePosition);
            if (distance < UiConstants.MoveSelectionStep)
                return;

            _lastMousePosition = evt.mousePosition;
            List<Cell> cells = _selector.GetCellsAtPosition(_lastMousePosition);
            Cell selected = cells.FirstOrDefault();
            if (selected == null || _selector.FirstSelectedCell == null)
                return;

            // For mouse move, use the Shift selection strategy.
            ISelectionStrategy strategy = SelectionStrategyFactory.GetSelectionStrategy<MultipleSelectionStrategy>();
            strategy.Preselect(_selector, new List<Cell> { selected });
            _selector.ConfirmSelection();
        }

        #endregion
        
        #region Keyboard Event Handlers
        
        private void OnKeyDown(KeyDownEvent evt)
        {
            if (!_selector.SelectionEnabled)
                return;

            Vector2 direction = GetArrowDirection(evt);
            if (direction != Vector2.zero)
            {
                ProcessArrowKey(direction);
            }
            else if (evt.keyCode is KeyCode.KeypadEnter or KeyCode.Return)
            {
                ProcessEnterKey();
            }
            else if (evt.keyCode is KeyCode.Backspace or KeyCode.Escape)
            {
                ProcessBackspaceOrEscape();
            }
            else if (_selector.FirstSelectedCell != null && evt.character is >= '!' and <= '~')
            {
                ProcessCharacterKey(evt);
            }
            else if (evt.keyCode == KeyCode.Tab)
            {
                ProcessTabKey(evt);
            }
            evt.StopPropagation();
        }
        
        #endregion
        
        #region Key Processing
        
        private void ProcessArrowKey(Vector2 direction)
        {
            if (_selector.FirstSelectedCell == null)
            {
                _selector.SelectFirstCellFromTable();
                return;
            }

            var contiguousCell = GetContiguousCell(direction);
            ISelectionStrategy strategy = SelectionStrategyFactory.GetSelectionStrategy<DefaultSelectionStrategy>();
            strategy.Preselect(_selector, new List<Cell> { contiguousCell });
            _selector.ConfirmSelection();
        }

        private void ProcessEnterKey()
        {
            if (_selector.FirstSelectedCell is SubTableCell subTableCell)
            {
                //If the subtable is not expanded, open it
                CellControl cellControl = CellControlFactory.GetCellControlFromId(_selector.FirstSelectedCell.Id);
                if (cellControl is ExpandableSubTableCellControl { IsFoldoutOpen: false } expandable)
                {
                    expandable.OpenFoldout();
                }
                
                //Select the first cell of the subtable
                Cell firstSubCell = subTableCell.SubTable.GetFirstCell();
                if (firstSubCell != null)
                {
                    _selector.FirstSelectedCell = firstSubCell;
                    _selector.ConfirmSelection();
                }
            }
            else if (_selector.FirstSelectedCell != null)
            {
                //If the cell is not a subtable, focus the field
                CellControl cellControl = CellControlFactory.GetCellControlFromId(_selector.FirstSelectedCell.Id);
                if (cellControl is ISimpleCellControl simpleCell && !simpleCell.IsFieldFocused())
                {
                    simpleCell.FocusField();
                }
            }
        }

        private void ProcessBackspaceOrEscape()
        {
            if(!_selector.FirstSelectedCell.Table.IsSubTable) return;
            
            _selector.FirstSelectedCell = _selector.FirstSelectedCell.Table.ParentCell;
            foreach (var descendant in _selector.FirstSelectedCell.GetDescendants())
            {
                _selector.CellsToDeselect.Add(descendant);
            }
            _selector.ConfirmSelection();
        }

        private void ProcessCharacterKey(KeyDownEvent evt)
        {
            CellControl cellControl = CellControlFactory.GetCellControlFromId(_selector.FirstSelectedCell.Id);
            if (cellControl is ITextBasedCellControl textBased && !textBased.IsFieldFocused())
            {
                textBased.SetValue(evt.character.ToString(), true);
            }
        }

        private void ProcessTabKey(KeyDownEvent evt)
        {
            if (_selector.FirstSelectedCell == null)
            {
                _selector.SelectFirstCellFromTable();
                return;
            }
            
            //If the user is focusing a field, do not change the selection
            CellControl cellControl = CellControlFactory.GetCellControlFromId(_selector.FirstSelectedCell.Id);
            if (cellControl is ISimpleCellControl simpleCell && simpleCell.IsFieldFocused())
                return;

            int orientation = evt.shiftKey ? -1 : 1;
            var ancestors = _selector.FirstSelectedCell.GetAncestors();
            ISelectionStrategy strategy = SelectionStrategyFactory.GetSelectionStrategy<DefaultSelectionStrategy>();

            if (_selector.SelectedCells.Count(x => !ancestors.Contains(x)) <= 1)
            {
                Cell contiguousCell = GetContiguousCell(Vector2.right * orientation);
                strategy.Preselect(_selector, new List<Cell> { contiguousCell });
                _selector.ConfirmSelection();
            }
            else
            {
                int index = _selector.OrderedSelectedCells.IndexOf(_selector.FirstSelectedCell) + orientation;
                if (index < 0) index = _selector.OrderedSelectedCells.Count - 1;
                else if (index >= _selector.OrderedSelectedCells.Count) index = 0;
                
                _selector.FirstSelectedCell = _selector.OrderedSelectedCells[index];
                
                //If the cell is a subtable and its closed, open the foldout
                if (_selector.FirstSelectedCell.Table.ParentCell is SubTableCell parentCell &&
                    !_tableControl.Metadata.IsTableExpanded(parentCell.Id))
                {
                    CellControl parentControl = CellControlFactory.GetCellControlFromId(parentCell.Id);
                    if (parentControl is ExpandableSubTableCellControl expandable)
                    {
                        expandable.OpenFoldout();
                    }
                }
            }
        }
        
        #endregion

        #region Helpers

        private bool IsValidClick(IMouseEvent evt)
        {
            return evt.button == 0 && _selector.SelectionEnabled;
        }
        
        private Vector2 GetArrowDirection(KeyDownEvent evt)
        {
            return evt.keyCode switch
            {
                KeyCode.UpArrow => Vector2.up,
                KeyCode.DownArrow => Vector2.down,
                KeyCode.LeftArrow => Vector2.left,
                KeyCode.RightArrow => Vector2.right,
                _ => Vector2.zero,
            };
        }
        
        private Cell GetContiguousCell(Vector2 direction)
        {
            Cell firstCell = _selector.FirstSelectedCell;
            Vector2 minBounds = new(1, 1);
            Vector2 maxBounds = new(firstCell.Table.Columns.Count, firstCell.Table.Rows.Count);

            if (firstCell.Table == _tableControl.TableData && _tableControl.Transposed)
            {
                direction = new Vector2(-direction.y, -direction.x);
            }

            Cell contiguousCell = CellLocator.GetContiguousCell(firstCell, direction, minBounds, maxBounds);
            return contiguousCell;
        }

        #endregion
    }
}