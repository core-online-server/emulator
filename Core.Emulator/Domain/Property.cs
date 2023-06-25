using Core.Emulator.Domain.Members.Properties;

namespace Core.Emulator.Domain
{
    public class Property
    {
        public Object Object { get; }

        public PropertyMember.PropertyMerge PropertyMerge { get; }

        public int Index { get; set; }

        public string Identifier { get; }

        public string CodeName { get; }

        public string Name { get; }

        public string Size { get; }

        public string Offset { get; set; }

        public Property(Object @object, PropertyMember.PropertyMerge propertyMerge, string identifier, string name, string size)
        {
            Object = @object;
            PropertyMerge = propertyMerge;
            Identifier = identifier;
            CodeName = identifier.Replace(".", string.Empty);
            Name = name;
            Size = size;
        }

        public override string ToString()
        {
            return $"{Index} {Identifier} {Size}";
        }
    }
}
