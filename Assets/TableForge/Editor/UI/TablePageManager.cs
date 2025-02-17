using UnityEngine;

namespace TableForge.UI
{
    internal class TablePageManager
    {
        private readonly TableControl _tableControl;
        private int _page = 0;
        private int _pageNumber;
        
        public int Page => _page;
        public int PageNumber => _pageNumber;
        public int FirstRowPosition => _pageNumber == 0 ? 0 : (Page - 1) * ToolbarData.PageSize + 1;
        public int LastRowPosition => Mathf.Min(Page * ToolbarData.PageSize, _tableControl.TableData.Rows.Count);
        
        public static int GetFirstRowPosition(int page, int rowsCount)
        {
            return rowsCount == 0 ? 0 : (page - 1) * ToolbarData.PageSize + 1;
        }
        
        public static int GetLastRowPosition(int page, int rowsCount)
        {
            return Mathf.Min(page * ToolbarData.PageSize, rowsCount);
        }
        
        public TablePageManager(TableControl tableControl)
        {
            _tableControl = tableControl;
            
            _pageNumber = tableControl.TableData.Rows.Count > 0 ? tableControl.TableData.Rows.Count / ToolbarData.PageSize + 1 : 0;
            _page = tableControl.TableData.Rows.Count > 0 ? 1 : 0;
            
            ToolbarData.OnPageSizeChanged += OnPageSizeChanged;
        }
        
        ~TablePageManager()
        {
            ToolbarData.OnPageSizeChanged -= OnPageSizeChanged;
        }
        
        private void OnPageSizeChanged()
        {
            _pageNumber = _tableControl.TableData.Rows.Count > 0 ? _tableControl.TableData.Rows.Count / ToolbarData.PageSize + 1 : 0;
            _page = _tableControl.TableData.Rows.Count > 0 ? 1 : 0;
            
            _tableControl.RefreshPage();
        }
        
        public void NextPage()
        {
            _page = 1 + (_page - 1 + ToolbarData.PageStep) % _pageNumber;
            _tableControl.RefreshPage();
        }
        
        public void PreviousPage()
        {
            _page = 1 + (_page - 1 - ToolbarData.PageStep + _pageNumber) % (_pageNumber);
            _tableControl.RefreshPage();
        }
    }
}