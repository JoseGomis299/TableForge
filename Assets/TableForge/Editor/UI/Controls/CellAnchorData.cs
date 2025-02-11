using System.Collections.Generic;

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
            
            float max = float.MinValue;
            foreach (var widths in _preferredWidths.Values)
            {
                if (widths > max)
                    max = widths;
            }
            PreferredWidth = max;
        }
        
        public void AddPreferredHeight(int id, float height)
        {
            if(!_preferredHeights.TryAdd(id, height))
                _preferredHeights[id] = height;

            float max = float.MinValue;
            foreach (var heights in _preferredHeights.Values)
            {
                if (heights > max)
                    max = heights;
            }
            PreferredHeight = max;
        }

        public CellAnchorData(CellAnchor cellAnchor)
        {
            CellAnchor = cellAnchor;
            AddPreferredHeight(0, UiContants.CellHeight);
        }
    }
}