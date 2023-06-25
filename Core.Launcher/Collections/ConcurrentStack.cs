using System.Collections;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Core.Abstract.Domain;
using Core.Launcher.Domain;
using Core.Launcher.Extensions;

namespace Core.Launcher.Collections;

public readonly unsafe struct ConcurrentStack<TEntity> : IProducerConsumerCollection<TEntity>
    where TEntity : IEntity
{
    internal int* Top { get; }

    internal IPool<TEntity> Pool { get; }

    internal int Next { get; }

    public ConcurrentStack(int* top, IPool<TEntity> pool, int next)
    {
        Top = top;
        Pool = pool;
        Next = next;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPop(out TEntity entity)
    {
        do
        {
            var top = Volatile.Read(ref *Top);

            entity = Pool.Get(top);

            if (top == 0) return false;

            if (Interlocked.CompareExchange(ref *Top, entity.GetInt32(Next), top) == top) return true;

        } while (true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(TEntity entity)
    {
        do
        {
            var top = Volatile.Read(ref *Top);

            entity.SetInt32(Next, top);

            if (Interlocked.CompareExchange(ref *Top, entity.Id, top) == top) return;

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
        Push(item);

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryTake(out TEntity item)
    {
        return TryPop(out item);
    }
}