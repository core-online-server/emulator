using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Core.Emulator.Domain.Members.Properties
{
    public class ArrayPropertyMember : EnumerablePropertyMember
    {
        private static readonly PropertyMergeDelegate PropertyMergeFactory = (o, n, t, tn, m) => new ArrayPropertyMerge(o, n, t, tn, m.Cast<ArrayPropertyMember>());

        public override PropertyMergeDelegate Merger => PropertyMergeFactory;

        public ArrayPropertyMember(Object @object, INamedTypeSymbol @interface, IPropertySymbol original) : base(@object, @interface, original)
        {
        }

        public static bool Is(IPropertySymbol original)
        {
            return Is(original, "IReadOnlyList", "Array");
        }

        public class ArrayPropertyMerge : EnumerablePropertyMerge<ArrayPropertyMember>
        {
            public Property Property { get; private set; }

            public ArrayPropertyMerge(Object @object, string name, string type, string typeName, IEnumerable<ArrayPropertyMember> members) : base(@object, name, type, typeName, members)
            {
            }

            public override string ResolveGetter()
            {
                return $"get => this.GetArray<Save, {Object.Name}, {Item.Name}>({ResolveOffset(Property)}, {Length});";
            }

            public override string ResolveSize()
            {
                return $"{Length} * {Item.Name}.Size";
            }

            public override IEnumerable<Property> ResolveProperties()
            {
                yield return Property = ResolveProperty();
            }
        }
    }
}
