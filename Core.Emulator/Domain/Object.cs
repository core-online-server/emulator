using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Core.Emulator.Domain.Members.Methods;
using Core.Emulator.Domain.Members.Properties;
using Core.Emulator.Extensions;
using Core.Generator.Extensions;
using Microsoft.CodeAnalysis;

namespace Core.Emulator.Domain
{
    public abstract class Object
    {
        public Root Root { get; }

        public string Name { get; }

        public ImmutableDictionary<ISymbol, ImmutableDictionary<ISymbol, string>> Dictionary { get; }

        public ImmutableList<INamedTypeSymbol> Interfaces { get; }

        public ImmutableList<INamedTypeSymbol> Handlers { get; }

        public IImmutableList<PropertyMember.PropertyMerge> PropertyMembers { get; set; }

        public IImmutableList<MethodMember.MethodMerge> MethodMembers { get; set; }

        public IImmutableList<Property> Properties { get; set; } = new Property[] { }.ToImmutableArray();

        public ImmutableDictionary<string, ImmutableList<Call>> Calls { get; set; } = new (string, Call[])[]{}.ToImmutableDictionary(v => v.Item1, v => v.Item2.ToImmutableList());

        public string Size { get; set; } = "0";

        protected Object(Root root, string name, ImmutableDictionary<ISymbol, ImmutableDictionary<ISymbol, string>> dictionary)
        {
            Root = root;
            Name = name;
            Dictionary = dictionary;
            Interfaces = Dictionary.Keys.OfType<INamedTypeSymbol>().Where(c => c.TypeKind == TypeKind.Interface).ToImmutableList();
            Handlers = Dictionary.Keys.OfType<INamedTypeSymbol>().Where(c => c.IsStatic && c.TypeKind == TypeKind.Class).ToImmutableList();
        }

        public static Object Create(Root root, string name,
            ImmutableDictionary<ISymbol, ImmutableDictionary<ISymbol, string>> dictionary)
        {
            if (dictionary.Keys.Any(i => i.GetAttributes().Any(a => a.IsElementAttribute())))
                return new Element(root, name, dictionary);

            if (dictionary.Keys.Any(i => i.GetAttributes().Any(a => a.IsEntityAttribute())))
                return new Entity(root, name, dictionary);

            throw new InvalidOperationException($"Invalid object {name}.");
        }

        public void AssignMembers()
        {
            PropertyMembers = Interfaces
                .SelectMany(i => i
                    .GetNestedProperties()
                    .Select(p => PropertyMember.Create(this, i, p)))
                .Where(m => m != null)
                .GroupBy(m => (m.Merger, m.Original.Name, m.Type, m.TypeName),
                    (k, l) => k.Merger(this, k.Name, k.Type, k.TypeName, l))
                .OrderBy(m => m.Name)
                .ToImmutableList();

            MethodMembers = Interfaces
                .SelectMany(i => i
                    .GetNestedMethods().Where(m => m.IsAbstract)
                    .SelectMany(p => MethodMember.Create(this, i, p)))
                .Concat(Handlers
                    .SelectMany(h => h
                        .GetNestedMethods().Where(m => m.IsStatic)
                        .SelectMany(p => MethodMember.Create(this, h, p))))
                .Where(m => m != null)
                .GroupBy(m => (m.Merger, m.GroupName, m.ReturnType, m.ReturnTypeName),
                    (k, l) => k.Merger(this, k.GroupName, k.ReturnType, k.ReturnTypeName, l))
                .OrderBy(m => m.Name)
                .ToImmutableList();
        }

        public override string ToString()
        {
            return Name;
        }

        public virtual IEnumerable<Property> MutateProperties(IEnumerable<Property> properties)
        {
            return properties.OrderBy(p => p.Identifier);
        }

        public virtual IEnumerable<Call> MutateCalls(IEnumerable<Call> calls)
        {
            return calls.OrderBy(c => c.Priority);
        }

        public void AssignMetaData()
        {
            var index = 0;

            Property last = null;

            foreach (var property in Properties)
            {
                property.Index = index++;

                property.Offset = last == null ? "0" : $"{last.CodeName}Offset + {last.Size}";

                last = property;
            }

            Size = last == null ? "0" : $"{last.CodeName}Offset + {last.Size}";
        }

        public abstract string GetCode();

        protected virtual string GetNamespaceCode()
        {
            return @"using System;
using Core.Launcher;
using Core.Launcher.Domain;
using Core.Launcher.Extensions;
using Core.Launcher.Collections;
using Core.Abstract.Domain;

namespace Launcher.Domain;";
        }

        protected virtual string GetInheritanceCode()
        {
            var inheritance = Interfaces
                .Select(i => i.ResolveTypeParameters(p => Dictionary[i][p]));

            return $@",
    {string.Join(@",
    ", inheritance)}";
        }

        protected virtual string GetPropertiesCode()
        {
            return !PropertyMembers.Any() ? string.Empty : $@"
{string.Join(@"
", PropertyMembers.Select(m => $@"
    public {m.ResolveType()} {m.Name}
    {{
        {m.ResolveGetter()}{(m.ResolveSetter() is var setter && string.IsNullOrEmpty(setter) ? string.Empty : $@"
        {m.ResolveSetter()}")}
    }}"))}";
        }

        protected virtual string GetMethodsCode()
        {
            return !MethodMembers.Any() ? string.Empty : $@"{string.Join(@"
", MethodMembers.Where(m => m.HasDeclaration()).Select(m => $@"
    public {m.ResolveDeclaration()}
    {{{GetMethodBodyCode(m, m.Parameters.ToImmutableHashSet())}
    }}"))}";
        }

        protected virtual string GetMethodBodyCode(MethodMember.MethodMerge method, ImmutableHashSet<(string fullType, string type, string name)> parameters)
        {
            if (!Calls.TryGetValue(method.Name, out var calls)) return string.Empty;

            var leasing = calls
                .Where(c => c.MethodMerge is ObjectMethodMember.ObjectMethodMerge)
                .SelectMany(c => c.Parameters.Skip(1).Where(p => c.Also.parameter != p.name))
                .Where(p => !parameters.Contains(p))
                .Distinct()
                .Select(p => $@"
        {p.type} {p.name} = Save.{p.type}Store.Lease();");

            return string.Join(@"
", leasing.Concat(GetSwitchCode(calls.Select(c => (c, c.Case)))));
        }

        private static IEnumerable<string> GetSwitchCode(IEnumerable<(Call call, Case @case)> calls, int nested = 0)
        {
            var indention = "        ";

            for (var i = 0; i < nested; i++) indention = $"        {indention}";

            var groups = calls
                .GroupBy(p => (p.call.Priority, p.@case?.Subject, p.@case?.Property))
                .OrderBy(g => g.Key.Priority)
                .Select(g => g.Key.Property == null ? string.Join(@"
", g.Select(p => $@"
{indention}{p.call.GetCode()}")) : $@"
{indention}switch({(g.Key.Subject == null ? string.Empty : $"{g.Key.Subject}.")}{g.Key.Property})
{indention}{{{string.Join(string.Empty, g.GroupBy(p => p.@case.Value).OrderBy(gc => gc.Key).Select(gc => $@"
{indention}    case {(gc.Key is bool ? gc.Key.ToString().ToLower() : gc.Key)}:
{indention}    {{{(gc.Count() == 1 && gc.Single().@case?.Nested == null ? $@"
{indention}        {gc.Single().call.GetCode()}" : string.Join(@"
", GetSwitchCode(gc.Select(p => (p.call, p.@case.Nested)), nested + 1)))}

{indention}        break;
{indention}    }}"))}
{indention}}}");

            return groups;
        }

        protected virtual string GetMetaCode()
        {
            return $@"
{string.Join(string.Empty, Properties.Select(p => $@"
    public const int {p.CodeName}Offset = {p.Offset};"))}

    private static Property[] _properties = new Property[]
    {{{string.Join(",", Properties.Select(p => $@"
        new({p.CodeName}Offset, {p.Size}, {p.Name})"))}
    }};

    public static Property[] GetProperties() => _properties;

    public const int Size = {Size};

    public static int GetSize() => Size;";
        }
    }
}
