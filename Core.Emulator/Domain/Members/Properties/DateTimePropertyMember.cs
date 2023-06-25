using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Core.Emulator.Domain.Members.Properties
{
    public class DateTimePropertyMember : PropertyMember
    {
        private static readonly PropertyMergeDelegate PropertyMergeFactory = (o, n, t, tn, m) => new DateTimePropertyMerge(o, n, t, tn, m.Cast<DateTimePropertyMember>());

        public override PropertyMergeDelegate Merger => PropertyMergeFactory;

        public DateTimePropertyMember(Object @object, INamedTypeSymbol @interface, IPropertySymbol original) : base(@object, @interface, original)
        {
        }

        public static bool Is(IPropertySymbol original)
        {
            return original.Type.Name == "DateTime";
        }

        public class DateTimePropertyMerge : PropertyMerge<DateTimePropertyMember>
        {
            public Property Property { get; private set; }

            public DateTimePropertyMerge(Object @object, string name, string type, string typeName, IEnumerable<DateTimePropertyMember> members) : base(@object, name, type, typeName, members)
            {
            }

            public override string ResolveGetter()
            {
                return $"get => new DateTime(this.GetInt64({ResolveOffset(Property)}));";
            }

            public override string ResolveSetter()
            {
                return $"set => this.SetInt64({ResolveOffset(Property)}, value.Ticks);";
            }

            public override string ResolveType()
            {
                return "DateTime";
            }

            public override string ResolveSize()
            {
                return "sizeof(long)";
            }

            public override IEnumerable<Property> ResolveProperties()
            {
                yield return Property = ResolveProperty();
            }
        }
    }
}
