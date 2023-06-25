namespace Core.Launcher.Domain;

public class Property
{
    public int Offset { get; set; }

    public int Size { get; }

    public string Name { get; }

    public Property(int offset, int size, string name)
    {
        Offset = offset;
        Size = size;
        Name = name;
    }

    public Property(int offset, int size, string @object, string subject, string property)
    {
        Offset = offset;
        Size = size;
        Name = $"{@object}.{subject}.{property}";
    }

    public Property(int offset, int size, string subject, string property)
    {
        Offset = offset;
        Size = size;
        Name = $"{subject}.{property}";
    }

    public override string ToString()
    {
        return $"{Name} {Size} at {Offset}";
    }
}