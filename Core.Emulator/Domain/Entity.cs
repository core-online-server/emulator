using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Core.Emulator.Extensions;
using Microsoft.CodeAnalysis;

namespace Core.Emulator.Domain
{
    public class Entity : Object
    {
        public bool Synchronized { get; }

        public Entity(Root root, string name, ImmutableDictionary<ISymbol, ImmutableDictionary<ISymbol, string>> dictionary) : base(root, name, dictionary)
        {
            Synchronized = dictionary.Keys.Any(i => i.GetAttributes().Any(a => a.IsSynchronizedAttribute()));
        }

        public override IEnumerable<Property> MutateProperties(IEnumerable<Property> properties)
        {
            return new[] { new Property(this, null, "Free", "nameof(Free)", "sizeof(int)") }
                .Concat(base.MutateProperties(properties));
        }

        public override string GetCode()
        {
            return $@"{GetNamespaceCode()}

public struct {Name} : IEntity<Save, {Name}>{GetInheritanceCode()}
{{  
    public Save Save {{ get; }}

    public int Id {{ get; set; }}

    public Pointer Pointer {{ get; }}

    public int Free
    {{
        get => this.GetInt32(FreeOffset);
        set => this.SetInt32(FreeOffset, value);
    }}{GetPropertiesCode()}

    public {Name}(Save save, int id, Pointer pointer)
    {{
        Save = save;
        Id = id;
        Pointer = pointer;
    }}{GetMethodsCode()}{GetMetaCode()}

    public static {Name} Create(Save save, int id, Pointer pointer)
    {{
        return new {Name}(save, id, pointer);
    }}

    public override string ToString()
    {{
        return $""{{Id}} {{base.ToString()}}"";
    }}
}}";
        }

        protected override string GetMetaCode()
        {
            return $@"{base.GetMetaCode()}
    public const int Capacity = 10000;

    public static int GetCapacity()
    {{
        return Capacity;
    }}";
        }
    }
}
