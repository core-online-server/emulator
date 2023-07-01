using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Core.Emulator.Extensions;
using Core.Generator.Extensions;
using Microsoft.CodeAnalysis;

namespace Core.Emulator.Domain.Members.Methods
{
    public abstract class MethodMember
    {
        public delegate MethodMerge MethodMergeDelegate(Object @object, string name, string returnType, string returnTypeName, IEnumerable<MethodMember> members);

        public Object Object { get; }

        public INamedTypeSymbol Interface { get; }

        public IMethodSymbol Original { get; }

        public string Name { get; }

        public virtual string GroupName => Original.IsAbstract ? Name : Name.Substring(2);

        public string ReturnType { get; }

        public string ReturnTypeName { get; }

        public string Parameters { get; }

        public ImmutableDictionary<ISymbol, string> Dictionary => Object.Dictionary[Interface];

        public abstract MethodMergeDelegate Merger { get; }

        protected MethodMember(Object @object, INamedTypeSymbol @interface, IMethodSymbol original)
        {
            Object = @object;

            Interface = @interface;

            Original = original;

            Name = original.Name;

            if (original.ReturnType.Kind == SymbolKind.TypeParameter)
            {
                ReturnType = ReturnTypeName = Dictionary[original.ReturnType];
            }
            else
            {
                ReturnType = $"{original.ReturnType}";

                ReturnTypeName = original.ReturnType.Name;
            }

            Parameters = string.Join(",", original.Parameters.Select(p => Dictionary.ResolveType(p.Type)));
        }

        public static IEnumerable<MethodMember> Create(Object @object, INamedTypeSymbol @interface, IMethodSymbol original)
        {
            if (GetMethodMember.Is(@object, @interface, original)) yield return new GetMethodMember(@object, @interface, original);

            else if (LeaseMethodMember.Is(@object, @interface, original)) yield return new LeaseMethodMember(@object, @interface, original);

            else if (ReleaseObjectMethodMember.Is(@object, @interface, original)) yield return new ReleaseObjectMethodMember(@object, @interface, original);

            else if (HoldMethodMember.Is(@object, @interface, original)) yield return new HoldMethodMember(@object, @interface, original);

            else if (ReleaseCachedMethodMember.Is(original)) yield return new ReleaseCachedMethodMember(@object, @interface, original);

            else if (ObjectMethodMember.Is(original))
            {
                var links = original
                    .GetAttributes()
                    .Where(a => a.IsAttribute("LinkAttribute"));

                foreach (var link in links)
                {
                    yield return new ObjectMethodMember(@object, @interface, original, link);
                }

                yield return new ObjectMethodMember(@object, @interface, original);
            }

            yield break;

            throw new InvalidOperationException($"Unknown {@interface} member {original.Name}");
        }

        public override string ToString()
        {
            return $"{ReturnType} {Name}({Parameters})";
        }

        public abstract class MethodMerge<T> : MethodMerge
            where T : MethodMember
        {
            public IImmutableSet<T> Members { get; }

            protected MethodMerge(Object @object, string name, string returnType, string returnTypeName, IEnumerable<T> members) : base(@object, name, returnType, returnTypeName)
            {
                Members = members.ToImmutableHashSet();

                Parameters = ResolveParameters().ToImmutableArray();
            }

            public override bool HasDeclaration()
            {
                return Members.Any(m => m.Original.IsAbstract);
            }

            private IEnumerable<(string @ref, string fullType, string type, string name)> ResolveParameters()
            {
                var declaration = Members
                    .Where(m => m.Original.IsAbstract)
                    .Select(m => string.Join(";", m.Original.Parameters.Select(p => ResolveParameter(m.Interface, p))))
                    .Distinct()
                    .ToImmutableList();

                if (declaration.Count > 1)
                    throw new InvalidOperationException($"Multiple declaration of {this.Name} of {this.Object.Name}.");

                var parameters = declaration
                    .SingleOrDefault() ?? string.Join(";", Members
                    .SelectMany(m => m.Original.Parameters.Select(p => ResolveParameter(m.Interface, p)))
                    .Distinct());

                return parameters
                    .Split(';')
                    .Select(ResolveParameterSplit)
                    .Where(s => s != default);
            }

            protected IEnumerable<(string @ref, string fullType, string type, string name)> ResolveParameters(T member)
            {
                return member.Original.Parameters
                    .Select(p => ResolveParameterSplit(ResolveParameter(member.Interface, p)))
                    .Where(s => s != default);
            }

            private static (string @ref, string fullType, string type, string name) ResolveParameterSplit(string parameters)
            {
                var split = parameters.Split(' ');

                return split.Length != 4 ? default : (split[0], split[1], split[2], split[3]);
            }

            private string ResolveParameter(ISymbol @interface, IParameterSymbol parameter)
            {
                var dictionary = Object.Dictionary[@interface];

                return dictionary.TryGetValue(parameter.Type, out var name) ? $"{(parameter.RefKind == RefKind.Ref ? "ref" : string.Empty)} {name} {name} {parameter.Name}" : $"{(parameter.RefKind == RefKind.Ref ? "ref" : string.Empty)} {parameter.Type} {parameter.Type.Name} {parameter.Name}";
            }
        }

        public abstract class MethodMerge
        {
            public Object Object { get; }

            public string Name { get; }

            public string ReturnType { get; }

            public string ReturnTypeName { get; }

            public ImmutableArray<(string @ref, string fullType, string type, string name)> Parameters { get; set; }

            protected MethodMerge(Object o, string name, string returnType, string returnTypeName)
            {
                Object = o;
                Name = name;
                ReturnType = returnType;
                ReturnTypeName = returnTypeName;
            }

            public abstract bool HasDeclaration();

            public virtual string ResolveDeclaration()
            {
                return $"{ReturnType} {Name}({string.Join(", ", Parameters.Select(p => $"{p.fullType} {p.name}"))})";
            }

            public IEnumerable<Call> ResolveCalls()
            {
                foreach (var call in ResolveSelfCalls().Concat(ResolveOtherCalls()))
                {
                    call.Object = Object;

                    call.MethodMerge = this;

                    if (call.Caller == null) call.Caller = Name;

                    yield return call;
                }
            }

            public abstract IEnumerable<Call> ResolveSelfCalls();

            public virtual IEnumerable<Call> ResolveOtherCalls(Object @object = null)
            {
                yield break;
            }

            public override string ToString()
            {
                return $"{ReturnType} {Name}({Parameters})";
            }
        }
    }
}
