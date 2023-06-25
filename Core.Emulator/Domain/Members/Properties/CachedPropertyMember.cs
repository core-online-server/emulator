﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Core.Emulator.Domain.Members.Properties
{
    public class CachedPropertyMember : PropertyMember
    {
        private static readonly PropertyMergeDelegate PropertyMergeFactory = (o, n, t, tn, m) => new CachedPropertyMerge(o, n, t, tn, m.Cast<CachedPropertyMember>());

        public override PropertyMergeDelegate Merger => PropertyMergeFactory;

        public CachedPropertyMember(Object @object, INamedTypeSymbol @interface, IPropertySymbol original) : base(@object, @interface, original)
        {
        }

        public static bool Is(IPropertySymbol original)
        {
            return original.Type.TypeKind == TypeKind.Class;
        }

        public class CachedPropertyMerge : PropertyMerge<CachedPropertyMember>
        {
            public Property Property { get; private set; }

            public CachedPropertyMerge(Object @object, string name, string type, string typeName, IEnumerable<CachedPropertyMember> members) : base(@object, name, type, typeName, members)
            {
            }

            public override string ResolveGetter()
            {
                return $"get => this.GetStored(Save.{TypeName}Store, {ResolveOffset(Property)});";
            }

            public override string ResolveSetter()
            {
                return $"set => this.SetStored(Save.{TypeName}Store, {ResolveOffset(Property)}, value);";
            }

            public override string ResolveSize()
            {
                return "sizeof(int)";
            }

            public override IEnumerable<Property> ResolveProperties()
            {
                yield return Property = ResolveProperty();
            }
        }
    }
}
