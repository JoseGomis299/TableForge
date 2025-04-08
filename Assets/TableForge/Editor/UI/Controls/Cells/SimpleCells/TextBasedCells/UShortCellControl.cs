using System;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(UShortCell), CellSizeCalculationMethod.AutoSize)]
    internal class UShortCellControl : TextBasedCellControl<int>
    {
        private const int MaxUShortChars = 5;
        public UShortCellControl(UShortCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = new IntegerField(MaxUShortChars)
            {
                value = (ushort)Cell.GetValue()
            };
            field.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue > ushort.MaxValue)
                {
                    field.SetValueWithoutNotify(ushort.MaxValue);
                    evt = ChangeEvent<int>.GetPooled(evt.previousValue, ushort.MaxValue);
                }
                else if (evt.newValue < ushort.MinValue)
                {
                    field.SetValueWithoutNotify(ushort.MinValue);
                    evt = ChangeEvent<int>.GetPooled(evt.previousValue, ushort.MinValue);
                }

                OnChange(evt, field);
            });
            OnRefresh = () => field.value = (ushort)Cell.GetValue();

            Add(field);
            TextField = field;

            field.AddToClassList(USSClasses.TableCellContent);
        }
        
        protected override void SetCellValue(object value)
        {
            ushort ushortValue = Convert.ToUInt16(value);
            Cell.SetValue(ushortValue);
        }
    }
}