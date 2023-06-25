using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Core.Emulator.Domain.Members.Properties
{
    public class ListPropertyMember : EnumerablePropertyMember
    {
        private static readonly PropertyMergeDelegate PropertyMergeFactory = (o, n, t, tn, m) => new ListPropertyMerge(o, n, t, tn, m.Cast<ListPropertyMember>());

        public override PropertyMergeDelegate Merger => PropertyMergeFactory;

        public ListPropertyMember(Object @object, INamedTypeSymbol @interface, IPropertySymbol original) : base(@object, @interface, original)
        {
        }

        public static bool Is(IPropertySymbol original)
        {
            return Is(original, "ICollection", "Collection");
        }

        public class ListPropertyMerge : EnumerablePropertyMerge<ListPropertyMember>
        {
            public Property BottomProperty { get; private set; }

            public Property CountProperty { get; private set; }

            public Property TopProperty { get; private set; }

            public Property NextProperty { get; private set; }

            public Property OwnerProperty { get; private set; }

            public Property PreviousProperty { get; private set; }

            public ListPropertyMerge(Object @object, string name, string type, string typeName, IEnumerable<ListPropertyMember> members) : base(@object, name, type, typeName, members)
            {
            }

            public override string ResolveGetter()
            {
                return $"get => this.GetList({ResolveOffset(BottomProperty)}, Save.{Item.Name}Store, {ResolveOffset(NextProperty)});";
            }

            public override string ResolveSize()
            {
                return "sizeof(int)";
            }

            public override IEnumerable<Property> ResolveProperties()
            {
                yield return BottomProperty = ResolveProperty(Object, "Bottom");

                yield return CountProperty = ResolveProperty(Object, "Count");

                yield return TopProperty = ResolveProperty(Object, "Top");

                yield return NextProperty = ResolveFullProperty(Item, "Next");

                yield return OwnerProperty = ResolveFullProperty(Item,  "Owner");

                yield return PreviousProperty = ResolveFullProperty(Item,  "Previous");
            }

            protected virtual Property ResolveFullProperty(Object @object, string name)
            {
                return ResolveFullProperty(@object, name, ResolveName(name));
            }

            protected virtual Property ResolveProperty(Object @object, string name)
            {
                return ResolveProperty(@object, name, ResolveName(name));
            }

            protected virtual string ResolveName(string name)
            {
                return $"List<{Item.Name}>.{name}";
            }
        }
    }
}