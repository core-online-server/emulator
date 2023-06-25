using System.Collections;
using System.Runtime.CompilerServices;
using Core.Abstract.Domain;
using Core.Abstract.Extensions;
using Core.Launcher.Domain;
using Core.Launcher.Extensions;

namespace Core.Launcher.Collections;

public readonly unsafe struct List<TChild> : ICollection<TChild>
    where TChild : IEntity
{
    public const int Bottom = 0;

    public int Count
    {
        get => *CountPointer;
        set => *CountPointer = value;
    }

    public const int Top = 2;

    public const int Next = 0;

    public const int Owner = 4;

    public const int Previous = 8;

    internal int Parent { get; }

    internal int* BottomPointer { get; }

    internal int* CountPointer => BottomPointer + 1;

    internal int* TopPointer => BottomPointer + Top;

    internal IStore<TChild> Store { get; }

    internal int NextOffset { get; }

    internal int OwnerOffset => NextOffset + Owner;

    internal int PreviousOffset => NextOffset + Previous;

    public bool IsReadOnly => false;

    internal List(int parent, int *parentPointer, IStore<TChild> children, int childrenOffset)
    {
        Parent = parent;
        BottomPointer = parentPointer;
        Store = children;
        NextOffset = childrenOffset;
    }

    public IEnumerator<TChild> GetEnumerator()
    {
        return new Enumerator<TChild>(Store, *TopPointer, NextOffset);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(TChild entity)
    {
        if (Contains(entity)) return;

        if (*TopPointer == 0)
        {
            *TopPointer = entity.Id;
        }
        else if (*BottomPointer > 0)
        {
            var existing = Store.Get(*BottomPointer);

            existing.SetInt32(NextOffset, entity.Id);

            entity.SetInt32(PreviousOffset, existing.Id);
        }

        *BottomPointer = entity.Id;

        entity.SetInt32(OwnerOffset, Parent);

        Count++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        foreach (var child in this)
        {
            Remove(child);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(TChild child)
    {
        if (Is.Default(child)) return false;

        return Parent == child.GetInt32(OwnerOffset);
    }

    public void CopyTo(TChild[] array, int arrayIndex)
    {
        var i = 0;

        foreach (var child in this)
        {
            array[arrayIndex + i] = child;

            i++;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(TChild child)
    {
        if (!Contains(child)) return false;

        var previous = ExchangePrevious(child, 0);

        var next = ExchangeNext(child, 0);

        if (previous.Id == 0) *TopPointer = 0;

        else if(next.Id > 0) previous.SetInt32(NextOffset, next.Id);

        else previous.SetInt32(NextOffset, 0);

        if (next.Id == 0) *BottomPointer = 0;

        else if(previous.Id > 0) next.SetInt32(PreviousOffset, previous.Id);

        else next.SetInt32(PreviousOffset, 0);

        child.SetInt32(OwnerOffset, 0);

        Count--;

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TChild ExchangePrevious(TChild child, int value)
    {
        return Store.Get(child.ExchangeInt32(PreviousOffset, value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TChild ExchangeNext(TChild child, int value)
    {
        return Store.Get(child.ExchangeInt32(NextOffset, value));
    }

    /*public int IndexOf(TChild item)
    {
        var i = 0;

        foreach (var child in this)
        {
            if (child.Id == item.Id) return i;

            i++;
        }

        return -1;
    }

    public void Insert(int index, TChild child)
    {
        var current = this[index];

        var previous = ExchangePrevious(current, child.Id);

        previous.SetInt32(Next, child.Id);

        child.SetInt32(Previous, previous.Id);

        child.SetInt32(Next, current.Id);
    }

    public void RemoveAt(int index)
    {
        var child = this[index];

        Remove(child);
    }

    public TChild this[int index]
    {
        get
        {
            var i = 0;

            foreach (var child in this)
            {
                if(i == index) return child;

                i++;
            }

            throw new IndexOutOfRangeException();
        }
        set
        {
            Remove(this[index]);

            Insert(index, value);
        }
    }*/
}