using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Core.Emulator.Domain.Members.Properties
{
    public class ConcurrentQueuePropertyMember : EnumerablePropertyMember
    {
        private static readonly PropertyMergeDelegate PropertyMergeFactory = (o, n, t, tn, m) => new ConcurrentQueuePropertyMerge(o, n, t, tn, m.Cast<ConcurrentQueuePropertyMember>());

        public override PropertyMergeDelegate Merger => PropertyMergeFactory;

        public ConcurrentQueuePropertyMember(Object @object, INamedTypeSymbol @interface, IPropertySymbol original) : base(@object, @interface, original)
        {
        }

        public static bool Is(IPropertySymbol original)
        {
            return Is(original, "IProducerConsumerCollection", "ConcurrentQueue");
        }

        public class ConcurrentQueuePropertyMerge : EnumerablePropertyMerge<ConcurrentQueuePropertyMember>
        {
            public Property BottomProperty { get; private set; }

            public Property TopProperty { get; private set; }

            public Property NextProperty { get; private set; }

            public ConcurrentQueuePropertyMerge(Object @object, string name, string type, string typeName, IEnumerable<ConcurrentQueuePropertyMember> members) : base(@object, name, type, typeName, members)
            {
            }

            public override string ResolveGetter()
            {
                return $"get => this.GetConcurrentQueue({ResolveOffset(BottomProperty)}, Save.{Item.Name}Store, {ResolveOffset(NextProperty)});";
            }

            public override string ResolveSize()
            {
                return "sizeof(int)";
            }

            public override IEnumerable<Property> ResolveProperties()
            {
                yield return BottomProperty = ResolveProperty(Object, "Bottom");

                yield return TopProperty = ResolveProperty(Object, "Top");

                yield return NextProperty = ResolveFullProperty(Item, "Next");
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
                return $"ConcurrentQueue<{Item.Name}>.{name}";
            }
        }
    }
}