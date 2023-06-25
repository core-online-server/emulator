using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Core.Emulator.Domain.Members.Methods
{
    public class HoldMethodMember : MethodMember
    {
        private static readonly MethodMergeDelegate MethodMergeFactory = (o, n, rt, rtn, m) => new HoldMethodMerge(o, n, rt, rtn, m.Cast<HoldMethodMember>());

        public override MethodMergeDelegate Merger => MethodMergeFactory;

        public HoldMethodMember(Object @object, INamedTypeSymbol @interface, IMethodSymbol original) : base(@object, @interface, original)
        {
        }

        public static bool Is(Object @object, INamedTypeSymbol @interface, IMethodSymbol original)
        {
            return original.Name.StartsWith("Hold") &&
                   original.Parameters.Length == 1 &&
                   original.Parameters.Single() is IParameterSymbol releaseParameter &&
                   !@object.Dictionary[@interface].ContainsKey(releaseParameter.Type);
        }

        public class HoldMethodMerge : MethodMerge<HoldMethodMember>
        {
            public HoldMethodMerge(Object @object, string name, string returnType, string returnTypeName, IEnumerable<HoldMethodMember> members) : base(@object, name, returnType, returnTypeName, members)
            {
            }

            public override IEnumerable<Call> ResolveSelfCalls()
            {
                yield return new Call($"Save.{Parameters.Single().type}Store.Hold", Parameters);
            }
        }
    }
}