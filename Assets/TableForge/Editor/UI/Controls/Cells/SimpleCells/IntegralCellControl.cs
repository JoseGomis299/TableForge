using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(IntegralCell), CellSizeCalculationMethod.AutoSize)]
    internal class IntegralCellControl : CellControl
    {
        public IntegralCellControl(IntegralCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = GetField();
            Add(field);
            
            field.AddToClassList(USSClasses.TableCellContent);
            IsSelected = false;
            
            InitializeSize();
        }
        
        private VisualElement GetField()
        {
            if (Cell.Type == typeof(int))
            {
                var field = new IntegerField
                {
                    value = (int)Cell.GetValue()
                };
                field.RegisterValueChangedCallback(evt => OnChange(evt, field));
                OnRefresh = () => field.value = (int)Cell.GetValue();
                return field;
            }

            if (Cell.Type == typeof(long))
            {
                var field = new LongField()
                {
                    value = (long)Cell.GetValue()
                };
                field.RegisterValueChangedCallback(evt => OnChange(evt, field));
                OnRefresh = () => field.value = (long)Cell.GetValue();
                return field;
            }
            
            if (Cell.Type == typeof(uint))
            {
                var field = new UnsignedIntegerField()
                {
                    value = (uint)Cell.GetValue()
                };
                field.RegisterValueChangedCallback(evt => OnChange(evt, field));
                OnRefresh = () => field.value = (uint)Cell.GetValue();
                return field;
            }
            
            if (Cell.Type == typeof(ulong))
            {
                var field = new UnsignedLongField()
                {
                    value = (ulong)Cell.GetValue()
                };
                field.RegisterValueChangedCallback(evt => OnChange(evt, field));
                OnRefresh = () => field.value = (ulong)Cell.GetValue();
                return field;
            }

            int maxValue = Cell.Type switch
            {
                { } t when t == typeof(byte) => byte.MaxValue,
                { } t when t == typeof(sbyte) => sbyte.MaxValue,
                { } t when t == typeof(short) => short.MaxValue,
                { } t when t == typeof(ushort) => ushort.MaxValue,
                _ => 0
            }; 
            
            int minValue = Cell.Type switch
            {
                { } t when t == typeof(byte) => byte.MinValue,
                { } t when t == typeof(sbyte) => sbyte.MinValue,
                { } t when t == typeof(short) => short.MinValue,
                { } t when t == typeof(ushort) => ushort.MinValue,
                _ => 0
            };

            //TODO: Add support for byte, sbyte, short, ushort
            var basefield = new IntegerField
            {
                value = (int)Cell.GetValue()
            };
            basefield.RegisterValueChangedCallback(evt => OnChange(evt, basefield));

            return basefield;
        }
    }
}