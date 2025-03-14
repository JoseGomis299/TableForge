using System;

namespace TableForge.UI
{
    internal interface IHeaderVisibilityNotifier
    {
        event Action<HeaderControl, int> OnHeaderBecameVisible;
        event Action<HeaderControl, int> OnHeaderBecameInvisible;
    }
}