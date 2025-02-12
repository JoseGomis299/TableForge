using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class TableCornerControl : HeaderControl
    {
        public TableCornerControl(TableControl tableControl) : base(0, "", tableControl)
        {
            AddToClassList("table__corner");
        }

    }
}