using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Core.Emulator.Domain.Members.Properties
{
    public class IdentityPropertyMember : PropertyMember
    {
        private static readonly PropertyMergeDelegate PropertyMergeFactory = (o, n, t, tn, m) => new IdentityPropertyMerge(o, n, t, tn, m.Cast<IdentityPropertyMember>());

        public override PropertyMergeDelegate Merger => PropertyMergeFactory;

        public IdentityPropertyMember(Object @object, INamedTypeSymbol @interface, IPropertySymbol original) : base(@object, @interface, original)
        {
        }

        public static bool Is(IPropertySymbol original)
        {
            return original.Name == "Identity" && original.Type.ToString() == "string";
        }

        public class IdentityPropertyMerge : PropertyMerge<IdentityPropertyMember>
        {
            public IdentityPropertyMerge(Object @object, string name, string type, string typeName, IEnumerable<IdentityPropertyMember> members) : base(@object, name, type, typeName, members)
            {
            }

            public override string ResolveGetter()
            {
                return $"get => \"{Object.Name}\";";
            }
        }
    }
}
