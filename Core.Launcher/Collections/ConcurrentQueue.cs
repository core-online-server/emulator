using System.Collections;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Core.Abstract.Domain;
using Core.Launcher.Domain;
using Core.Launcher.Extensions;

namespace Core.Launcher.Collections;

public readonly unsafe struct ConcurrentQueue<TEntity> : IProducerConsumerCollection<TEntity>
    where TEntity : IEntity
{
    public const int Bottom = 0;

    public const int Top = 1;

    public const int Next = 0;

    internal int* BottomPointer { get; }

    internal int* TopPointer => BottomPointer + Top;

    internal IStore<TEntity> Store { get; }

    internal int NextOffset { get; }

    public ConcurrentQueue(int* parentPointer, IStore<TEntity> store, int childrenOffset)
    {
        BottomPointer = parentPointer;
        Store = store;
        NextOffset = childrenOffset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryDequeue(out TEntity entity)
    {
        do
        {
            var top = Volatile.Read(ref *TopPointer);

            if (top == -1) continue;

            entity = Store.Get(top);

            if (top == 0) return false;

            if (Interlocked.CompareExchange(ref *TopPointer, entity.GetInt32(NextOffset), top) != top) continue;

            entity.SetInt32(NextOffset, 0);

            return true;

        } while (true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Enqueue(TEntity entity)
    {
        do
        {
            var top = Volatile.Read(ref *TopPointer);

            if (top == -1 || Interlocked.CompareExchange(ref *TopPointer, -1, top) != top) continue;

            if (top == 0)
            {
                top = entity.Id;
            }
            else if (*BottomPointer > 0)
            {
                var existing = Store.Get(*BottomPointer);

                existing.SetInt32(NextOffset, entity.Id);
            }

            *BottomPointer = entity.Id;

            Volatile.Write(ref *TopPointer, top);

            return;

        } while (true);
    }

    public IEnumerator<TEntity> GetEnumerator()
    {
        throw new NotSupportedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotSupportedException();
    }

    public void CopyTo(Array array, int index)
    {
        throw new NotSupportedException();
    }

    public int Count => throw new NotSupportedException();

    public bool IsSynchronized => true;

    public object SyncRoot => throw new NotSupportedException();

    public void CopyTo(TEntity[] array, int index)
    {
        throw new NotSupportedException();
    }

    public TEntity[] ToArray()
    {
        throw new NotSupportedException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryAdd(TEntity item)
    {
        Enqueue(item);

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryTake(out TEntity item)
    {
        return TryDequeue(out item);
    }
}