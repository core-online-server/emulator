using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Core.Emulator.Extensions
{
    public static class PropertyExtensions
    {
        public static IEnumerable<IPropertySymbol> GetNestedProperties(this INamedTypeSymbol type)
        {
            foreach (var property in type.GetMembers().OfType<IPropertySymbol>())
            {
                yield return property;
            }

            foreach (var @interface in type.AllInterfaces)
            {
                foreach (var property in @interface.GetMembers().OfType<IPropertySymbol>())
                {
                    yield return property;
                }
            }
        }
    }
}
