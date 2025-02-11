using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class TableCornerControl : VisualElement
    {
        private readonly TableControl _tableControl;

        public TableCornerControl(TableControl tableControl)
        {
            _tableControl = tableControl;

            AddToClassList("table__corner");

        }

        void BuildHeader()
        {




        }

    }
}