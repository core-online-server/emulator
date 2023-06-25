using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Core.Generator.Extensions;
using Microsoft.CodeAnalysis;

namespace Core.Emulator.Domain.Members.Properties
{
    public class StoredPropertyMember : PropertyMember
    {
        private static readonly PropertyMergeDelegate PropertyMergeFactory = (o, n, t, tn, m) => new ObjectPropertyMerge(o, n, t, tn, m.Cast<StoredPropertyMember>());

        public override PropertyMergeDelegate Merger => PropertyMergeFactory;

        public AttributeData LinkAttribute { get; }

        public StoredPropertyMember(Object @object, INamedTypeSymbol @interface, IPropertySymbol original) : base(@object, @interface, original)
        {
            LinkAttribute = original.GetAttributes().SingleOrDefault(a => a.IsAttribute("LinkAttribute"));
        }

        public static bool Is(IPropertySymbol original)
        {
            return original.Type.Kind == SymbolKind.TypeParameter;
        }

        public class ObjectPropertyMerge : PropertyMerge<StoredPropertyMember>
        {
            public Property Property { get; private set; }

            public string Link { get; set; }

            public ObjectPropertyMerge(Object @object, string name, string type, string typeName, IEnumerable<StoredPropertyMember> members) : base(@object, name, type, typeName, members)
            {
                var source = Members
                    .GroupBy(m => m.Original, SymbolEqualityComparer.Default)
                    .Select(g => g.First())
                    .ToImmutableArray();

                var linkedSource = source
                    .Where(m => m.LinkAttribute != null)
                    .Where(m => m.LinkAttribute.ConstructorArguments.Length == 1)
                    .ToImmutableArray();

                var links = linkedSource
                    .Select(m => m.LinkAttribute.ConstructorArguments[0].Value)
                    .OfType<string>()
                    .ToImmutableArray();

                if (links.Length > 1)
                {
                    foreach (var member in linkedSource)
                    {
                        Object.Root.Context.ReportDiagnostic(
                            Diagnostic.Create(
                                new DiagnosticDescriptor(
                                    "LG0005",
                                    "Multiple link attributes",
                                    "Multiple link attributes detected {0}",
                                    "Attributes",
                                    DiagnosticSeverity.Error,
                                    true),
                                null,
                                $"{member.Original}"));
                    }
                }

                Link = links.SingleOrDefault();
            }

            public override string ResolveGetter()
            {
                var property = GetProperty();

                return $"get => this.GetStored(Save.{Type}Store, {ResolveOffset(property)});";
            }

            public override string ResolveSetter()
            {
                var property = GetProperty();

                return $"set => this.SetStored(Save.{Type}Store, {ResolveOffset(property)}, value);";
            }

            private Property GetProperty()
            {
                return Link == null ? Property : Object.Properties.Single(p => p.Identifier == Link);
            }

            public override string ResolveSize()
            {
                return "sizeof(int)";
            }

            public override IEnumerable<Property> ResolveProperties()
            {
                if (Link != null) yield break;

                yield return Property = ResolveProperty();
            }
        }
    }
}