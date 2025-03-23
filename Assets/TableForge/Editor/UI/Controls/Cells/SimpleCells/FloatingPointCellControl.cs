using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(FloatingPointCell), CellSizeCalculationMethod.AutoSize)]
    internal class FloatingPointCellControl : CellControl
    {
        public FloatingPointCellControl(FloatingPointCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = GetField();
            Add(field);
            
            field.AddToClassList(USSClasses.TableCellContent);
        }
        
        private VisualElement GetField()
        {
            if (Cell.Type == typeof(float))
            {
                var field = new FloatField
                {
                    value = (float)Cell.GetValue()
                };
                field.RegisterValueChangedCallback(evt => OnChange(evt, field));
                OnRefresh = () => field.value = (float)Cell.GetValue();
                return field;
            }

            if (Cell.Type == typeof(double))
            {
                var field = new DoubleField
                {
                    value = (double)Cell.GetValue()
                };
                field.RegisterValueChangedCallback(evt => OnChange(evt, field));
                OnRefresh = () => field.value = (double)Cell.GetValue();
                return field;
            }

            return null;
        }
    }
}