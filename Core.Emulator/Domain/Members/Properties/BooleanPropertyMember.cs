using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Core.Generator.Extensions;
using Microsoft.CodeAnalysis;

namespace Core.Emulator.Domain.Members.Properties
{
    public class BooleanPropertyMember : PropertyMember
    {
        private static readonly PropertyMergeDelegate PropertyMergeFactory = (o, n, t, tn, m) => new BooleanPropertyMerge(o, n, t, tn, m.Cast<BooleanPropertyMember>());

        public override PropertyMergeDelegate Merger => PropertyMergeFactory;

        public AttributeData FlagAttribute { get; }

        public BooleanPropertyMember(Object @object, INamedTypeSymbol @interface, IPropertySymbol original) : base(@object, @interface, original)
        {
            FlagAttribute = original.GetAttributes().SingleOrDefault(a => a.IsAttribute("FlagAttribute"));
        }

        public static bool Is(IPropertySymbol original)
        {
            return original.Type.Name == "Boolean";
        }

        public class BooleanPropertyMerge : PropertyMerge<BooleanPropertyMember>
        {
            public (string name, int index) Flag { get; }

            public Property Property { get; private set; }

            public BooleanPropertyMerge(Object @object, string name, string type, string typeName, IEnumerable<BooleanPropertyMember> members) : base(@object, name, type, typeName, members)
            {
                var source = Members
                    .GroupBy(m => m.Original, SymbolEqualityComparer.Default)
                    .Select(g => g.First())
                    .ToImmutableArray();

                var flaggedSource = source
                    .Where(m => m.FlagAttribute != null)
                    .Where(m => m.FlagAttribute.ConstructorArguments.Length == 2)
                    .ToImmutableArray();

                var flags = flaggedSource
                    .Where(m => m.FlagAttribute.ConstructorArguments[0].Value is string flag && !string.IsNullOrEmpty(flag) && m.FlagAttribute.ConstructorArguments[1].Value is int)
                    .Select(m => ((string)m.FlagAttribute.ConstructorArguments[0].Value, m.FlagAttribute.ConstructorArguments[1].Value is int value ? value : 0))
                    .ToImmutableArray();

                if (flags.Length > 1)
                {
                    foreach (var member in flaggedSource)
                    {
                        Object.Root.Context.ReportDiagnostic(
                            Diagnostic.Create(
                                new DiagnosticDescriptor(
                                    "LG0004",
                                    "Multiple flag attributes",
                                    "Multiple flag attributes detected {0}",
                                    "Attributes",
                                    DiagnosticSeverity.Error,
                                    true),
                                null,
                                $"{member.Original}"));
                    }
                }

                Flag = flags.SingleOrDefault();
            }

            public override string ResolveGetter()
            {
                return Property != null
                    ? $"get => this.Get{TypeName}({ResolveOffset(Property)});"
                    : $"get => this.GetFlag({GetFlagPropertyOffset()}, {Flag.index});";
            }

            public override string ResolveSetter()
            {
                return Property != null
                    ? $"set => this.Set{TypeName}({ResolveOffset(Property)}, value);"
                    : $"set => this.SetFlag({GetFlagPropertyOffset()}, {Flag.index}, value);";
            }

            private string GetFlagPropertyOffset()
            {
                return ResolveOffset(Object.Properties.Single(p => p.Identifier == Flag.name));
            }

            public override IEnumerable<Property> ResolveProperties()
            {
                if (Flag == default) yield return Property = ResolveProperty();
            }
        }
    }
}
