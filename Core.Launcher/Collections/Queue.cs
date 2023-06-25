using System.Collections;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Core.Abstract.Domain;
using Core.Launcher.Domain;
using Core.Launcher.Extensions;

namespace Core.Launcher.Collections;

public readonly unsafe struct Queue<TEntity> : IProducerConsumerCollection<TEntity>
    where TEntity : IEntity
{
    internal int* Top { get; }

    internal int* Bottom => Top + 1;

    internal IPool<TEntity> Pool { get; }

    internal int Next { get; }

    public Queue(int* top, IPool<TEntity> pool, int next)
    {
        Top = top;
        Pool = pool;
        Next = next;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryDequeue(out TEntity entity)
    {
        entity = Pool.Get(*Top);

        if (*Top == 0) return false;

        *Top = entity.ExchangeInt32(Next, 0);

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Enqueue(TEntity entity)
    {
        if (*Top == 0)
        {
            *Top = entity.Id;
        }
        else if (*Bottom > 0)
        {
            var existing = Pool.Get(*Bottom);

            existing.SetInt32(Next, entity.Id);
        }

        *Bottom = entity.Id;
    }

    public IEnumerator<TEntity> GetEnumerator()
    {
        return new Enumerator<TEntity>(Pool, *Top, Next);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void CopyTo(Array array, int index)
    {
        var i = 0;

        foreach (var child in this)
        {
            array.SetValue(child, index + i);

            i++;
        }
    }

    public int Count
    {
        get
        {
            var i = 0;

            foreach (var _ in this)
            {
                i++;
            }

            return i;
        }
    }

    public bool IsSynchronized => false;

    public object SyncRoot => null!;

    public void CopyTo(TEntity[] array, int index)
    {
        var i = 0;

        foreach (var child in this)
        {
            array[index + i] = child;

            i++;
        }
    }

    public TEntity[] ToArray()
    {
        var array = new TEntity[Count];

        CopyTo(array, 0);

        return array;
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