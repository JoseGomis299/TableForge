using System;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(ShortCell), CellSizeCalculationMethod.AutoSize)]
    internal class ShortCellControl : TextBasedCellControl<int>
    {
        private const int MaxShortChars = 6;
        public ShortCellControl(ShortCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = new IntegerField(MaxShortChars)
            {
                value = (short)Cell.GetValue()
            };
            field.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue > short.MaxValue)
                {
                    field.SetValueWithoutNotify(short.MaxValue);
                    evt = ChangeEvent<int>.GetPooled(evt.previousValue, short.MaxValue);
                }
                else if (evt.newValue < short.MinValue)
                {
                    field.SetValueWithoutNotify(short.MinValue);
                    evt = ChangeEvent<int>.GetPooled(evt.previousValue, short.MinValue);
                }

                OnChange(evt, field);
            });
            OnRefresh = () => field.value = (short)Cell.GetValue();

            Add(field);
            TextField = field;

            field.AddToClassList(USSClasses.TableCellContent);
        }
        
        protected override void SetCellValue(object value)
        {
            short shortValue = Convert.ToInt16(value);
            Cell.SetValue(shortValue);
        }
    }
}