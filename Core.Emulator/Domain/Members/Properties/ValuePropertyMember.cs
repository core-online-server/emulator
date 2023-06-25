using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Core.Emulator.Domain.Members.Properties
{
    public class ValuePropertyMember : PropertyMember
    {
        private static readonly PropertyMergeDelegate PropertyMergeFactory = (o, n, t, tn, m) => new ValuePropertyMerge(o, n, t, tn, m.Cast<ValuePropertyMember>());

        public override PropertyMergeDelegate Merger => PropertyMergeFactory;

        public ValuePropertyMember(Object @object, INamedTypeSymbol @interface, IPropertySymbol original) : base(@object, @interface, original)
        {
        }

        public static bool Is(IPropertySymbol original)
        {
            return original.Type.IsValueType && original.Name != "Id";
        }

        public class ValuePropertyMerge : PropertyMerge<ValuePropertyMember>
        {
            public Property Property { get; private set; }

            public ValuePropertyMerge(Object @object, string name, string type, string typeName, IEnumerable<ValuePropertyMember> members) : base(@object, name, type, typeName, members)
            {
            }

            public override string ResolveGetter()
            {
                return $"get => this.Get{TypeName}({ResolveOffset(Property)});";
            }

            public override string ResolveSetter()
            {
                return $"set => this.Set{TypeName}({ResolveOffset(Property)}, value);";
            }

            public override IEnumerable<Property> ResolveProperties()
            {
                yield return Property = ResolveProperty();
            }
        }
    }
}
