using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Core.Emulator.Domain.Members.Methods
{
    public class ReleaseCachedMethodMember : MethodMember
    {
        private static readonly MethodMergeDelegate MethodMergeFactory = (o, n, rt, rtn, m) => new ReleaseCachedMethodMerge(o, n, rt, rtn, m.Cast<ReleaseCachedMethodMember>());

        public override MethodMergeDelegate Merger => MethodMergeFactory;

        public ReleaseCachedMethodMember(Object @object, INamedTypeSymbol @interface, IMethodSymbol original) : base(@object, @interface, original)
        {
        }

        public static bool Is(IMethodSymbol original)
        {
            return original.Name.StartsWith("Release") &&
                   original.Parameters.Length == 1 &&
                   original.Parameters.Single() is IParameterSymbol releaseParameter &&
                   releaseParameter.Type.TypeKind == TypeKind.Class;
        }

        public class ReleaseCachedMethodMerge : MethodMerge<ReleaseCachedMethodMember>
        {
            public ReleaseCachedMethodMerge(Object @object, string name, string returnType, string returnTypeName, IEnumerable<ReleaseCachedMethodMember> members) : base(@object, name, returnType, returnTypeName, members)
            {
            }

            public override IEnumerable<Call> ResolveSelfCalls()
            {
                yield return new Call($"Save.{Parameters.Single().type}Store.Release", Parameters);
            }
        }
    }
}