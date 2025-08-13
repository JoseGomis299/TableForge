using System.Collections.Generic;
using System.Linq;
using TableForge.Editor.Serialization;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
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
            _selector.PreselectCells(_lastMousePosition, evt.ctrlKey, evt.shiftKey, evt.button == 0);
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (evt.pressedButtons != 1 || !IsValidClick(evt) || evt.button != 0)
                return;

            float distance = Vector3.Distance(evt.mousePosition, _lastMousePosition);
            if (distance < UiConstants.MoveSelectionStep)
                return;

            _lastMousePosition = evt.mousePosition;
            PreselectArguments preselectArgs = _selector.GetCellPreselectArgsForPosition(_lastMousePosition);
            Cell selected = preselectArgs.cellsAtPosition.FirstOrDefault();
            if (selected == null || _selector.FocusedCell == null)
                return;

            // For mouse move, use the Shift selection strategy.
            ISelectionStrategy strategy = SelectionStrategyFactory.GetSelectionStrategy<MultipleSelectionStrategy>();
            strategy.Preselect(preselectArgs);
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
                if(_selector.FocusedCell == null)
                    return;
                
                ProcessBackspaceOrEscape(evt);
            }
            else if (_selector.FocusedCell != null && evt.character is >= '!' and <= '~')
            {
                if (evt.character == '=') ProcessEqualsKey(evt);
                else ProcessCharacterKey(evt);
            }
            else if (evt.keyCode == KeyCode.Tab)
            {
                ProcessTabKey(evt);
            }
            else if(evt.ctrlKey && evt.keyCode == KeyCode.C)
            {
                ProcessCopyKey(evt.shiftKey);
            }
            else if(evt.ctrlKey && evt.keyCode == KeyCode.V)
            {
                ProcessPasteKey(evt.shiftKey);
            }
            else if(evt.ctrlKey && evt.keyCode == KeyCode.X)
            {
                ProcessCutKey();
            }
            else if(evt.ctrlKey && evt.keyCode == KeyCode.Z)
            {
                UndoRedoManager.Undo(out var relatedCells);
                if (relatedCells is { Count: > 0 })
                {
                    _selector.ClearSelection();
                    _selector.SelectRange(relatedCells);
                }
                
                _tableControl.Visualizer.ToolbarController.RefreshFunctionTextField();
                _tableControl.schedule.Execute(() =>
                {
                    _tableControl.Update(true);
                    _tableControl.FunctionExecutor.ExecuteAllFunctions();
                }).ExecuteLater(0);
            }
            else if(evt.ctrlKey && evt.keyCode == KeyCode.Y)
            {
                UndoRedoManager.Redo(out var relatedCells);
                if (relatedCells is { Count: > 0 })
                {
                    _selector.ClearSelection();
                    _selector.SelectRange(relatedCells);
                }
                
                _tableControl.Visualizer.ToolbarController.RefreshFunctionTextField();
                _tableControl.schedule.Execute(() =>
                {
                    _tableControl.Update(true);
                    _tableControl.FunctionExecutor.ExecuteAllFunctions();
                }).ExecuteLater(0);
            }
            evt.StopPropagation();
        }
        
        #endregion
        
        #region Key Processing
        
        private void ProcessArrowKey(Vector2 direction)
        {
            if (_selector.FocusedCell == null)
            {
                _selector.SelectFirstCellFromTable();
                return;
            }

            var contiguousCell = GetContiguousCell(direction);
            ISelectionStrategy strategy = SelectionStrategyFactory.GetSelectionStrategy<DefaultSelectionStrategy>();
            strategy.Preselect(new PreselectArguments
            {
                selector = _selector,
                cellsAtPosition = new List<Cell> { contiguousCell }
            });
            _selector.ConfirmSelection();
        }

        private void ProcessEnterKey()
        {
            if (_selector.FocusedCell is SubTableCell subTableCell)
            {
                //If the subtable is not expanded, open it
                CellControl cellControl = CellControlFactory.GetCellControlFromId(_selector.FocusedCell.Id);
                if (cellControl is ExpandableSubTableCellControl { IsFoldoutOpen: false } expandable)
                {
                    expandable.OpenFoldout();
                }
                
                //Select the first cell of the subtable
                Cell firstSubCell = subTableCell.SubTable.GetFirstCell();
                if (firstSubCell != null)
                {
                    _selector.FocusedCell = firstSubCell;
                    _selector.ConfirmSelection();
                }
            }
            else if (_selector.FocusedCell != null)
            {
                //If the cell is not a subtable, focus the field
                CellControl cellControl = CellControlFactory.GetCellControlFromId(_selector.FocusedCell.Id);
                if (cellControl is ISimpleCellControl simpleCell && !simpleCell.IsFieldFocused())
                {
                    simpleCell.FocusField();
                }
            }
        }

        private void ProcessBackspaceOrEscape(KeyDownEvent evt)
        {
            if (!_selector.FocusedCell.Table.IsSubTable)
            {
                if (evt.shiftKey)
                {
                    ExpandableSubTableCellControl cellControl = CellControlFactory.GetCellControlFromId(_selector.FocusedCell.Id) as ExpandableSubTableCellControl;
                    cellControl?.CloseFoldout();
                }

                return;
            }
            
            if(evt.shiftKey)
            {
                ExpandableSubTableCellControl cellControl = CellControlFactory.GetCellControlFromId(_selector.FocusedCell.Id) as ExpandableSubTableCellControl;
                cellControl?.CloseFoldout();
            }
            
            _selector.FocusedCell = _selector.FocusedCell.Table.ParentCell;
            foreach (var descendant in _selector.FocusedCell.GetDescendants())
            {
                _selector.CellsToDeselect.Add(descendant);
            }
            _selector.ConfirmSelection();
        }

        private void ProcessCharacterKey(KeyDownEvent evt)
        {
            CellControl cellControl = CellControlFactory.GetCellControlFromId(_selector.FocusedCell.Id);
            if (cellControl is ITextBasedCellControl textBased && !textBased.IsFieldFocused())
            {
                textBased.SetValue(evt.character.ToString(), true);
            }
        }

        private void ProcessTabKey(KeyDownEvent evt)
        {
            if (_selector.FocusedCell == null)
            {
                _selector.SelectFirstCellFromTable();
                return;
            }
            
            //If the user is focusing a field, do not change the selection
            CellControl cellControl = CellControlFactory.GetCellControlFromId(_selector.FocusedCell.Id);
            if (cellControl is ISimpleCellControl simpleCell && simpleCell.IsFieldFocused())
                return;

            int orientation = evt.shiftKey ? -1 : 1;
            var ancestors = _selector.FocusedCell.GetAncestors();
            ISelectionStrategy strategy = SelectionStrategyFactory.GetSelectionStrategy<DefaultSelectionStrategy>();

            if (_selector.SelectedCells.Count(x => !ancestors.Contains(x)) <= 1)
            {
                Cell contiguousCell = GetContiguousCell(Vector2.right * orientation);
                strategy.Preselect(new PreselectArguments
                {
                    selector = _selector,
                    cellsAtPosition = new List<Cell> { contiguousCell }
                });
                _selector.ConfirmSelection();
            }
            else
            {
                Cell nextCell = _selector.CellNavigator.GetNextCell(orientation);
                
                //If the cell is a subtable and its closed, open the foldout
                if (nextCell.Table.ParentCell is SubTableCell parentCell &&
                    !_tableControl.Metadata.IsTableExpanded(parentCell.Id))
                {
                    CellControl parentControl = CellControlFactory.GetCellControlFromId(parentCell.Id);
                    if (parentControl is ExpandableSubTableCellControl expandable)
                    {
                        expandable.OpenFoldout();
                    }
                }
                
                _selector.FocusedCell = nextCell;
            }
        }
        
        private void ProcessCopyKey(bool copyFunctions)
        {
            if (_selector.FocusedCell == null)
                return;
            
            CopyBuffer.Copy(_selector.SelectedCells.ToList(), _tableControl.Metadata, copyFunctions, SerializationOptionsFactory.GetOptions(SerializationFormat.Default));
        }
        
        private void ProcessPasteKey(bool copyFunctions)
        {
            if (_selector.FocusedCell == null)
                return;
            
            (Cell first, Cell last) = CopyBuffer.Paste(_selector.SelectedCells.ToList(), _tableControl, copyFunctions, SerializationOptionsFactory.GetOptions(SerializationFormat.Default));
            if (first != null && last != null)
            {
                _selector.ClearSelection();
                _selector.SelectRange(first, last);
            }
            
            _tableControl.schedule.Execute(() =>
            {
                _tableControl.Update(true);
                _tableControl.FunctionExecutor.ExecuteAllFunctions();
            }).ExecuteLater(0);
        }
        
        private void ProcessCutKey()
        {
            if (_selector.FocusedCell == null)
                return;
        }
        
        private void ProcessEqualsKey(KeyDownEvent evt)
        {
            if (_selector.FocusedCell == null)
                return;
            
            _tableControl.Visualizer.ToolbarController.FocusFunctionText();
        }
        
        
        #endregion

        #region Helpers

        private bool IsValidClick(IMouseEvent evt)
        {
            return (_selector.SelectionEnabled);
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
            Cell firstCell = _selector.FocusedCell;
            Vector2 minBounds = new(1, 1);
            Vector2 maxBounds = new(firstCell.Table.Columns.Count, firstCell.Table.Rows.Count);

            if (firstCell.Table == _tableControl.TableData && _tableControl.Transposed)
            {
                direction = new Vector2(-direction.y, -direction.x);
            }


            Cell contiguousCell = firstCell;
            do
            {
                contiguousCell = CellLocator.GetContiguousCell(contiguousCell, direction, minBounds, maxBounds);
            } while ((!_tableControl.Metadata.IsFieldVisible(contiguousCell.column.Id) 
                     || !_tableControl.Filterer.IsVisible(contiguousCell.row.GetRootAnchor().Id))
                     && contiguousCell != firstCell);

            return contiguousCell;
        }

        #endregion
    }
}