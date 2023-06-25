using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Core.Emulator.Extensions
{
    public static class SymbolExtensions
    {
        public static IEnumerable<IAssemblySymbol> GetReferencedAssemblies(this Compilation compilation)
        {
            return compilation.References
                .Select(compilation.GetAssemblyOrModuleSymbol)
                .OfType<IAssemblySymbol>();
        }

        public static string ResolveTypeParameters(this INamedTypeSymbol symbol, Func<ITypeParameterSymbol, string> resolver)
        {
            return !symbol.TypeParameters.Any() ? $"{symbol}" : $"{symbol.ContainingNamespace}.{symbol.Name}<{string.Join(", ", symbol.TypeParameters.Select(resolver))}>";
        }

        public static string ResolveType(this ImmutableDictionary<ISymbol, string> dictionary, ITypeSymbol type)
        {
            return type.Kind == SymbolKind.TypeParameter ? dictionary[type] : $"{type}";
        }
    }
}
