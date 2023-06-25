using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Core.Emulator.Domain
{
    public class Element : Object
    {
        public Element(Root root, string name, ImmutableDictionary<ISymbol, ImmutableDictionary<ISymbol, string>> dictionary) : base(root, name, dictionary)
        {
        }

        public override string GetCode()
        {
            return $@"{GetNamespaceCode()}

public struct {Name} : IElement<Save, {Name}>{GetInheritanceCode()}
{{  
    public Save Save {{ get; }}

    public Pointer Pointer {{ get; }}{GetPropertiesCode()}

    public {Name}(Save save, Pointer pointer)
    {{
        Save = save;
        Pointer = pointer;
    }}{GetMethodsCode()}{GetMetaCode()}

    public static {Name} Create(Save save, Pointer pointer)
    {{
        return new {Name}(save, pointer);
    }}
}}";
        }
    }
}
