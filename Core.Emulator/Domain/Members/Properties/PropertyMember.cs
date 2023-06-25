using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Core.Emulator.Domain.Members.Properties
{
    public abstract class PropertyMember
    {
        public delegate PropertyMerge PropertyMergeDelegate(Object @object, string name, string type, string typeName, IEnumerable<PropertyMember> members);

        public Object Object { get; }

        public INamedTypeSymbol Interface { get; }

        public IPropertySymbol Original { get; }

        public string Type { get; }

        public string TypeName { get; }

        public ImmutableDictionary<ISymbol, string> Dictionary => Object.Dictionary[Interface];

        public abstract PropertyMergeDelegate Merger { get; }

        protected PropertyMember(Object @object, INamedTypeSymbol @interface, IPropertySymbol original)
        {
            Object = @object;

            Interface = @interface;

            Original = original;

            if (original.Type.Kind == SymbolKind.TypeParameter)
            {
                Type = TypeName = Dictionary[original.Type];
            }
            else
            {
                Type = $"{original.Type}";

                TypeName = original.Type.Name;
            }
        }

        public static PropertyMember Create(Object @object, INamedTypeSymbol @interface, IPropertySymbol original)
        {
            if (IdentityPropertyMember.Is(original)) return new IdentityPropertyMember(@object, @interface, original);

            if (SpanPropertyMember.Is(original)) return new SpanPropertyMember(@object, @interface, original);

            if (BooleanPropertyMember.Is(original)) return new BooleanPropertyMember(@object, @interface, original);

            if (DateTimePropertyMember.Is(original)) return new DateTimePropertyMember(@object, @interface, original);

            if (ValuePropertyMember.Is(original)) return new ValuePropertyMember(@object, @interface, original);

            if (ConcurrentQueuePropertyMember.Is(original)) return new ConcurrentQueuePropertyMember(@object, @interface, original);

            if (ListPropertyMember.Is(original)) return new ListPropertyMember(@object, @interface, original);

            if (ArrayPropertyMember.Is(original)) return new ArrayPropertyMember(@object, @interface, original);

            if (StoredPropertyMember.Is(original)) return new StoredPropertyMember(@object, @interface, original);

            if (CachedPropertyMember.Is(original)) return new CachedPropertyMember(@object, @interface, original);

            return null;

            throw new InvalidOperationException($"Unknown {@interface} member {original.Name}");
        }

        public override string ToString()
        {
            return $"{Original.Type} {Original.Name}";
        }

        public abstract class PropertyMerge<T> : PropertyMerge
            where T : PropertyMember
        {
            public IImmutableSet<T> Members { get; }

            protected PropertyMerge(Object @object, string name, string type, string typeName, IEnumerable<T> members) : base(@object, name, type, typeName)
            {
                Members = members.ToImmutableHashSet();
            }
        }

        public abstract class PropertyMerge
        {
            public Object Object { get; }

            public string Name { get; }

            public string Type { get; }

            public string TypeName { get; }

            protected PropertyMerge(Object @object, string name, string type, string typeName)
            {
                Object = @object;

                Name = name;

                Type = type;

                TypeName = typeName;
            }

            public virtual string ResolveType()
            {
                return $"{Type}";
            }

            public virtual string ResolveSize()
            {
                return $"sizeof({Type})";
            }

            public abstract string ResolveGetter();

            public virtual string ResolveSetter()
            {
                return null;
            }

            protected string ResolveOffset(Property property)
            {
                return $"{property.Object.Name}.{property.CodeName}Offset";
            }

            protected Property ResolveProperty()
            {
                return new Property(Object, this, Name, $"nameof({Name})", ResolveSize());
            }

            protected virtual Property ResolveProperty(Object @object, string identifier, string name)
            {
                return new Property(@object, this, $"{Name}.{identifier}", $"nameof({Name}), nameof({name})", ResolveSize());
            }

            protected virtual Property ResolveFullProperty(Object @object, string identifier, string name)
            {
                return new Property(@object, this, $"{Object.Name}.{Name}.{identifier}", $"nameof({Object.Name}), nameof({Object.Name}.{Name}), nameof({name})", ResolveSize());
            }

            public virtual IEnumerable<Property> ResolveProperties()
            {
                yield break;
            }

            public override string ToString()
            {
                return $"{Type} {Name}";
            }
        }
    }
}
