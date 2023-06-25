using System.Runtime.CompilerServices;
using Core.Launcher.Collections;
using Core.Launcher.Domain;

namespace Core.Launcher.Extensions;

public static class ElementExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TElement GetElement<TSave, TObject, TElement>(this TObject @object, int offset)
        where TObject : IObject<TSave>
        where TElement : IElement<TSave, TElement>
    {
        return TElement.Create(@object.Save, @object.Pointer.Offset(offset));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void SetElement<TObject, TElement>(this TObject @object, int offset, TElement element)
        where TObject : IObject
        where TElement : IElement
    {
        var size = TElement.GetSize();

        Buffer.MemoryCopy(element.Pointer.Value, @object.Pointer.Value + offset, size, size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Array<TSave, TChild> GetArray<TSave, TParent, TChild>(this TParent parent, int offset, int count)
        where TParent : IObject<TSave>
        where TChild : IElement<TSave, TChild>
    {
        return new Array<TSave, TChild>(parent.Save, parent.Pointer.Offset(offset), count);
    }
}