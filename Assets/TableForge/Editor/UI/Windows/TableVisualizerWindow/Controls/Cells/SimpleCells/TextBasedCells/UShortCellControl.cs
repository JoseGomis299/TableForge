using System;
using TableForge.Editor.UI.UssClasses;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
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

            Add(field);
            TextField = field;

            field.AddToClassList(TableVisualizerUss.TableCellContent);
        }

        protected override void OnRefresh()
        {
            TextField.value = (ushort)Cell.GetValue();
        }

        protected override void SetCellValue(object value)
        {
            ushort ushortValue = Convert.ToUInt16(value);
            base.SetCellValue(ushortValue);
        }
    }
}