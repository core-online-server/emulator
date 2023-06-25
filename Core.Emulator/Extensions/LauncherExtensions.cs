using System.Linq;
using Core.Generator.Extensions;
using Microsoft.CodeAnalysis;

namespace Core.Emulator.Extensions
{
    public static class LauncherExtensions
    {
        public static bool HasLauncherAttribute(this IAssemblySymbol symbol)
        {
            return symbol.GetAttributes().Any(IsLauncherAttribute);
        }

        public static bool IsLauncherAttribute(this AttributeData attribute)
        {
            return attribute.IsAttribute("LauncherAttribute");
        }
    }
}
