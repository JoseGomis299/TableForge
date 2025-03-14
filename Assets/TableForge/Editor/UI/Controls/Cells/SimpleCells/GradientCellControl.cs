using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(GradientCell), CellSizeCalculationMethod.FixedBigCell)]
    internal class GradientCellControl : CellControl
    {
        public GradientCellControl(GradientCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = new GradientField()
            {
                value = (Gradient)Cell.GetValue()
            };
            field.RegisterValueChangedCallback(evt =>
            {
                //We need to create a new Gradient to avoid the reference being shared between cells when re-utilizing this cellControl
                Gradient newGradient = new Gradient();
                newGradient.alphaKeys = evt.newValue.alphaKeys.ToArray();
                newGradient.colorKeys = evt.newValue.colorKeys.ToArray();
                
                var cachedEvt = ChangeEvent<Gradient>.GetPooled(evt.previousValue, newGradient);
                OnChange(cachedEvt, field);
            });            
            OnRefresh = () => field.value = (Gradient)Cell.GetValue();
            Add(field);
            
            field.AddToClassList(USSClasses.TableCellContent);
            IsSelected = false;
            
            InitializeSize();
        }
    }
}