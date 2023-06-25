using System.Runtime.CompilerServices;

namespace Core.Launcher.Domain;

public interface IEntity<TSave, TEntity> : IEntity<TEntity>, IObject<TSave>
    where TEntity : IEntity<TSave, TEntity>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static abstract TEntity Create(TSave save, int id, Pointer pointer);
}

public interface IEntity<TEntity> : IEntity
    where TEntity : IEntity<TEntity>
{
    public static abstract Property[] GetProperties();

    public static abstract int GetCapacity();
}

public interface IEntity : IObject
{
    int Id { get; }

    public int Free { get; set; }
}