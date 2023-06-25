using System.Text;
using Core.Launcher.Comparators;

namespace Core.Launcher.Domain;

public class Schema
{
    private Property[]? _collection;

    internal string Name { get; }

    public int Size { get; private set; }

    public const int Offset = 4;

    internal Schema(string name)
    {
        Name = name;
    }

    internal Schema Clone()
    {
        if (_collection == null)
            throw new InvalidOperationException("Schema is not initialized.");

        var schema = new Schema(Name);

        var properties = new Property[_collection.Length];

        for (var i = 0; i < _collection.Length; i++)
        {
            properties[i] = new Property(_collection[i].Offset, _collection[i].Size, _collection[i].Name);
        }

        schema._collection = properties;

        schema.Size = Size;

        return schema;
    }

    internal void ReadFromCode(Property[] properties, int size)
    {
        _collection = properties;

        Size = size;
    }

    internal void ReadFromFile(string path)
    {
        if (!File.Exists(path))
            throw new InvalidOperationException("Schema path not found.");

        using var stream = File.OpenRead(path);

        using var reader = new BinaryReader(stream, Encoding.UTF8);

        var count = reader.ReadInt32();

        _collection = new Property[count];

        var offset = 0;

        for (var i = 0; i < count; i++)
        {
            var name = reader.ReadString();

            var size = reader.ReadByte();

            var property = new Property(-1, size, name)
            {
                Offset = offset
            };

            _collection[i] = property;

            offset += size;
        }

        Size = offset;
    }

    internal Property[] GetProperties()
    {
        if (_collection == null)
            throw new InvalidOperationException("Schema is not initialized.");

        return _collection;
    }

    internal void Write(string path)
    {
        if (_collection == null)
            throw new InvalidOperationException("Schema is not initialized.");

        using var stream = File.Open(path, FileMode.Create);

        using var writer = new BinaryWriter(stream, Encoding.UTF8);

        var count = _collection.Length;

        writer.Write(count);

        for (var i = 0; i < count; i++)
        {
            var property = _collection[i];

            writer.Write(property.Name);

            writer.Write((byte)property.Size);
        }
    }

    internal static bool Compare(Schema left, Schema right)
    {
        var leftProperties = left.GetProperties();

        var rightProperties = right.GetProperties();

        if (leftProperties.Length != rightProperties.Length) return false;

        var comparer = PropertyComparer.Instance;

        for (var i = 0; i < leftProperties.Length; i++)
        {
            if (!comparer.Equals(leftProperties[i], rightProperties[i]))
                return false;
        }

        return true;
    }

    internal static Schema CreateTransferSchema(Schema source, Schema target)
    {
        var transfer = target.Clone();

        var sourceProperties = source.GetProperties();

        var transferProperties = transfer.GetProperties();

        var hashset = sourceProperties.ToHashSet(new PropertyComparer());

        transfer.Size = 0;

        foreach (var property in transferProperties)
        {
            if (!hashset.TryGetValue(property, out var found)) property.Offset = -1;

            else
            {
                property.Offset = found.Offset;

                transfer.Size += found.Size;
            }
        }

        return transfer;
    }
}