using System;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    [CellControlUsage(typeof(ByteCell), CellSizeCalculationMethod.AutoSize)]
    internal class ByteCellControl : TextBasedCellControl<int>
    {
        private const int MaxByteChars = 3;
        public ByteCellControl(ByteCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = new IntegerField(MaxByteChars)
            {
                value = (byte)Cell.GetValue()
            };
            field.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue > byte.MaxValue)
                {
                    field.SetValueWithoutNotify(byte.MaxValue);
                    evt = ChangeEvent<int>.GetPooled(evt.previousValue, byte.MaxValue);
                }
                else if (evt.newValue < byte.MinValue)
                {
                    field.SetValueWithoutNotify(byte.MinValue);
                    evt = ChangeEvent<int>.GetPooled(evt.previousValue, byte.MinValue);
                }

                OnChange(evt, field);
            });
            OnRefresh = () => field.value = (byte)Cell.GetValue();

            Add(field);
            TextField = field;

            field.AddToClassList(USSClasses.TableCellContent);
        }

        protected override void SetCellValue(object value)
        {
            byte byteValue = Convert.ToByte(value);
            base.SetCellValue(byteValue);
        }
    }
}