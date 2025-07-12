using System.Collections.Generic;

namespace TableForge.Editor.UI
{
    internal interface ICellBoundCommand
    {
        Cell BoundCell { get; }
    }

    internal interface IAssetBoundCommand
    {
        List<string> Guids { get; }
    }
}