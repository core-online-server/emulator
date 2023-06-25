using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using Core.Abstract.Domain;
using Core.Launcher.Extensions;

namespace Core.Launcher.Domain;

public class Pool<TSave, TEntity> : Pool<TEntity>, IPool<TEntity>/*, IReadOnlyList<TEntity>, IProducerConsumerCollection<TEntity>*/
    where TSave : Save<TSave>, ISave<TSave>
    where TEntity : IEntity<TSave, TEntity>
{
    public TSave Save { get; }

    /*public int Count => Length;

    public object SyncRoot => throw new NotSupportedException();*/

    public Pool(TSave save, Schema schema, string? label, bool isSynchronized) : base(schema, label, isSynchronized)
    {
        Save = save;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe TEntity Lease()
    {
        if (IsSynchronized)
        {
            do
            {
                var free = Volatile.Read(ref *Free);

                if (free == 0) break;

                var entity = Create(free);

                if (Interlocked.CompareExchange(ref *Free, entity.Free, free) == free) return entity;

            } while (true);

            return Create(Interlocked.Increment(ref Length));
        }
        else
        {
            if (*Free == 0) return Create(++Length);

            var entity = Create(*Free);

            *Free = entity.Free;

            return entity;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Get(TEntity entity)
    {
        return entity.Id;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TEntity Get(int id)
    {
        return id == 0 ? default! : TEntity.Create(Save, id, GetPointer(id));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TEntity Create(int id)
    {
        return TEntity.Create(Save, id, GetPointer(id));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Pointer GetPointer(int id)
    {
        return Pointer.Offset(Schema.Offset + (id - 1) * TEntity.GetSize());
    }

    public void Flush()
    {
        var path = Save.GetPoolPath<TEntity>();

        WriteToFile(path);
    }

    /*public IEnumerator<TEntity> GetEnumerator()
    {
        if(IsSynchronized) throw new NotSupportedException();

        return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        if (IsSynchronized) throw new NotSupportedException();

        return GetEnumerator();
    }

    public void CopyTo(Array array, int index)
    {
        if (IsSynchronized) throw new NotSupportedException();

        var i = 0;

        foreach (var child in this)
        {
            array.SetValue(child, index + i);

            i++;
        }
    }

    public TEntity this[int index] => GetValue(index);


    public void CopyTo(TEntity[] array, int index)
    {
        if (IsSynchronized) throw new NotSupportedException();

        var i = 0;

        foreach (var child in this)
        {
            array[index + i] = child;

            i++;
        }
    }

    public TEntity[] ToArray()
    {
        if (IsSynchronized) throw new NotSupportedException();

        var array = new TEntity[Count];

        CopyTo(array, 0);

        return array;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryAdd(TEntity item)
    {
        Release(item);

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryTake(out TEntity item)
    {
        item = Lease();

        return true;
    }

    public class Enumerator : IEnumerator<TEntity>
    {
        private int _index;

        private readonly IPool<TEntity> _pool;

        public TEntity Current => _pool.GetValue(_index);

        object IEnumerator.Current => Current;

        public Enumerator(IPool<TEntity> pool)
        {
            _pool = pool;
        }

        public bool MoveNext()
        {
            do
            {
                _index++;

            } while (_index < _pool.Count && Current.Free > 0);

            return _index < _pool.Count;
        }

        public void Reset()
        {
            _index = 0;
        }

        public void Dispose()
        {
        }
    }*/
}

public abstract class Pool<TEntity> : Pool
    where TEntity : IEntity<TEntity>
{
    internal Pool(Schema schema, string? label, bool isSynchronized) : base(schema, Schema.Offset + schema.Size * TEntity.GetCapacity(), label, isSynchronized)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void Release(TEntity entity)
    {
        if (IsSynchronized)
        {
            do
            {
                var free = Volatile.Read(ref *Free);

                entity.Free = free;

                if (Interlocked.CompareExchange(ref *Free, entity.Id, free) == free) return;

            } while (true);
        }
        else
        {
            entity.Free = *Free;

            *Free = entity.Id;
        }
    }
}

public abstract class Pool : IDisposable
{
#if DEBUG
    public unsafe Span<byte> Array => new (Pointer.Value, Length * Size);
#endif
    internal string? Label { get; }

    public Schema Schema { get; }

    public Property[] Properties { get; }

    private readonly unsafe byte* _pointer;

    public unsafe Pointer Pointer => new (_pointer);

    internal int Size { get; }

    protected int Length;

    internal unsafe int* Free { get; }

    internal MemoryMappedFile Memory { get; }

    internal MemoryMappedViewAccessor Accessor { get; }

    public bool IsSynchronized { get; }

    internal Pool(Schema schema, int size, string? label, bool isSynchronized)
    {
        Label = label;
        Schema = schema;
        Properties = schema.GetProperties();
        Memory = MemoryMappedFile.CreateNew($"{schema.Name}{Label}Pool", size);
        Accessor = Memory.CreateViewAccessor();
        Size = size;
        unsafe
        {
            Accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref _pointer);
            Free = (int*)Pointer.Value;
        }
        IsSynchronized = isSynchronized;
    }

    internal void ReadFromFile(string path)
    {
        using var from = File.OpenRead(path);

        using var to = Memory.CreateViewStream(0, (int)from.Length);

        from.CopyTo(to);

        Length = ((int)from.Length - 4) / Schema.Size;
    }

    internal void WriteToFile(string path)
    {
        using var from = Memory.CreateViewStream(0, Schema.Offset + Length * Schema.Size);

        using var to = File.Open(path, FileMode.Create);

        from.CopyTo(to);
    }

    internal static void Transfer<TSave, TEntity>(Pool<TSave, TEntity> fromPool, Pool<TSave, TEntity> toPool)
        where TSave : Save<TSave>, ISave<TSave>
        where TEntity : IEntity<TSave, TEntity>
    {
        toPool.Length = fromPool.Length;

        var fromProperties = fromPool.Schema.GetProperties();

        var toProperties = toPool.Schema.GetProperties();

        for (var i = 0; i < fromPool.Length; i++)
        {
            var fromEntity = fromPool.Get(i + 1);

            var toEntity = toPool.Get(i + 1);

            for (var j = 0; j < fromProperties.Length; j++)
            {
                var fromProperty = fromProperties[j];

                var toProperty = toProperties[j];

                if (fromProperty.Offset < 0 || toProperty.Offset < 0) continue;

                unsafe
                {
                    void* from = fromEntity.GetPointer(j);

                    void* to = toEntity.GetPointer(j);

                    Buffer.MemoryCopy(from, to, toPool.Size, fromProperty.Size);
                }
            }
        }
    }

    public void Dispose()
    {
        Accessor.SafeMemoryMappedViewHandle.ReleasePointer();

        Accessor.Dispose();

        Memory.Dispose();
    }
}