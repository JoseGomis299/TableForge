using System.Collections.Generic;
using System.Linq;

namespace TableForge.UI
{
    internal class CellAnchorData
    {
        public float PreferredWidth { get; private set; }
        public float PreferredHeight { get; private set; }
        public CellAnchor CellAnchor { get; }

        public int Position => CellAnchor?.Position ?? 0;
        public int Id => CellAnchor?.Id ?? 0;

        private Dictionary<int, float> _preferredWidths = new Dictionary<int, float>();
        private Dictionary<int, float> _preferredHeights = new Dictionary<int, float>();
        
        public void AddPreferredWidth(int id, float width)
        {
            if(!_preferredWidths.TryAdd(id, width))
                _preferredWidths[id] = width;
            
            PreferredWidth = _preferredWidths.Values.Max();
        }
        
        public void AddPreferredHeight(int id, float height)
        {
            if(!_preferredHeights.TryAdd(id, height))
                _preferredHeights[id] = height;

            PreferredHeight = _preferredHeights.Values.Max();
        }

        public CellAnchorData(CellAnchor cellAnchor)
        {
            CellAnchor = cellAnchor;
            AddPreferredHeight(0, UiConstants.CellHeight);
        }
    }
}