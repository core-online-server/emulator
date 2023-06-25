using System.Runtime.CompilerServices;
using Core.Abstract.Domain;
using Core.Launcher.Collections;
using Core.Launcher.Domain;

namespace Core.Launcher.Extensions;

public static class EntityExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ConcurrentQueue<TChild> GetConcurrentQueue<TParent, TChild>(this TParent parent, int parentOffset, IStore<TChild> children, int childrenOffset)
        where TParent : IEntity
        where TChild : IEntity
    {
        return new ConcurrentQueue<TChild>(parent.GetInt32Pointer(parentOffset), children, childrenOffset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Collections.List<TChild> GetList<TParent, TChild>(this TParent parent, int parentOffset, IStore<TChild> children, int childrenOffset)
        where TParent : IEntity
        where TChild : IEntity
    {
        return new Collections.List<TChild>(parent.Id, parent.GetInt32Pointer(parentOffset), children, childrenOffset);
    }
}