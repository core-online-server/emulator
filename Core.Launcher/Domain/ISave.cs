namespace Core.Launcher.Domain;

public interface ISave<TSave> : IDisposable
    where TSave : Save<TSave>, ISave<TSave>
{
    public static abstract Pool<TSave, TEntity> CreatePool<TEntity>(TSave save, Schema schema, string? label = null, bool isSynchronized = false) where TEntity : IEntity<TSave, TEntity>;
}