namespace Core.Launcher.Domain;

public abstract class Save<TSave> : IDisposable
    where TSave : Save<TSave>, ISave<TSave>
{
    private readonly string _path;

    protected Save(string path)
    {
        _path = path;
    }

    public abstract void Flush();

    public void Dispose()
    {
        Flush();
    }

    protected Pool<TSave, TEntity> ReadPool<TEntity>(TSave save, bool isSynchronized = false)
        where TEntity : IEntity<TSave, TEntity>
    {
        var poolPath = GetPoolPath<TEntity>();

        var schemaPath = GetSchemaPath<TEntity>();

        var fileSchema = ReadFileSchema<TEntity>(schemaPath);

        var codeSchema = ReadCodeSchema<TEntity>();

        if (!Schema.Compare(fileSchema, codeSchema))
        {
            var transferSchema = Schema.CreateTransferSchema(fileSchema, codeSchema);

            codeSchema.Write(schemaPath);

            var fromPool = TSave.CreatePool<TEntity>(save, transferSchema, nameof(Pool.Transfer));

            if (File.Exists(poolPath)) fromPool.ReadFromFile(poolPath);

            var toPool = TSave.CreatePool<TEntity>(save, codeSchema, isSynchronized: isSynchronized);

            Pool.Transfer(fromPool, toPool);

            toPool.WriteToFile(poolPath);

            return toPool;
        }
        else
        {
            var pool = TSave.CreatePool<TEntity>(save, codeSchema, isSynchronized: isSynchronized);

            pool.ReadFromFile(poolPath);

            return pool;
        }
    }

    internal Schema ReadFileSchema<T>(string path)
    {
        var schema = new Schema(typeof(T).Name);

        if (File.Exists(path)) schema.ReadFromFile(path);

        else schema.ReadFromCode(Array.Empty<Property>(), 0);

        return schema;
    }

    internal Schema ReadCodeSchema<T>()
        where T : IEntity<T>
    {
        var schema = new Schema(typeof(T).Name);

        schema.ReadFromCode(T.GetProperties(), T.GetSize());

        return schema;
    }

    internal string GetSchemaPath<T>()
    {
        var name = typeof(T).Name;

        return Path.Combine(_path, $"{name}.sch");
    }

    internal string GetPoolPath<T>()
    {
        var name = typeof(T).Name;

        return Path.Combine(_path, $"{name}.bin");
    }
}