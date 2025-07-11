using System;
using TableForge.Editor.UI.UssClasses;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    [CellControlUsage(typeof(SByteCell), CellSizeCalculationMethod.AutoSize)]
    internal class SByteCellControl : TextBasedCellControl<int>
    {
        private const int MaxSByteChars = 4;
        public SByteCellControl(SByteCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = new IntegerField(MaxSByteChars)
            {
                value = (sbyte)Cell.GetValue()
            };
            field.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue > sbyte.MaxValue)
                {
                    field.SetValueWithoutNotify(sbyte.MaxValue);
                    evt = ChangeEvent<int>.GetPooled(evt.previousValue, sbyte.MaxValue);
                }
                else if (evt.newValue < sbyte.MinValue)
                {
                    field.SetValueWithoutNotify(sbyte.MinValue);
                    evt = ChangeEvent<int>.GetPooled(evt.previousValue, sbyte.MinValue);
                }

                OnChange(evt, field);
            });

            Add(field);
            TextField = field;

            field.AddToClassList(TableVisualizerUss.TableCellContent);
        }
        
        protected override void SetCellValue(object value)
        {
            sbyte sbyteValue = Convert.ToSByte(value);
            base.SetCellValue(sbyteValue);
        }
    }
}