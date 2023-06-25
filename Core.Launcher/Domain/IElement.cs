using System.Runtime.CompilerServices;

namespace Core.Launcher.Domain;

public interface IElement<TSave, out TElement> : IElement, IObject<TSave>
    where TElement : IElement<TSave, TElement>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static abstract TElement Create(TSave save, Pointer pointer);
}

public interface IElement : IObject
{
}