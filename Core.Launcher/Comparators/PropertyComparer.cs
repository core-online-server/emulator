using Core.Launcher.Domain;

namespace Core.Launcher.Comparators;

internal class PropertyComparer : IEqualityComparer<Property>
{
    public static readonly PropertyComparer Instance = new();

    public bool Equals(Property? x, Property? y)
    {
        if (ReferenceEquals(x, y)) return true;

        if (x is null) return false;

        if (y is null) return false;

        if (x.GetType() != y.GetType()) return false;

        return x.Name == y.Name && x.Size == y.Size;
    }

    public int GetHashCode(Property obj)
    {
        return HashCode.Combine(obj.Name, obj.Size);
    }
}