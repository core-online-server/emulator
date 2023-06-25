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
    public class Root
    {
        public GeneratorExecutionContext Context { get; private set; }

        public ImmutableDictionary<string, Object> Objects { get; set; }

        public ImmutableList<(string name, bool synchronized)> Pools { get; set; }

        public ImmutableList<(string fullType, string type)> Caches { get; set; }

        public void Resolve(GeneratorExecutionContext context)
        {
            Context = context;

            Objects = CreateObjects().ToImmutableDictionary(o => o.Name, o => o);

            AssignMembers();

            AssignProperties();

            AssignCalls();

            Pools = Objects.Values.OfType<Entity>().Select(o => (o.Name, o.Synchronized)).OrderBy(o => o.Name).ToImmutableList();

            Caches = GetCached().Distinct().OrderBy(c => c.type).ToImmutableList();
        }

        private IEnumerable<Object> CreateObjects()
        {
            var assemblies = Context.Compilation.GetReferencedAssemblies().ToImmutableList();

            var constructs = assemblies
                .SelectMany(a => a.GetEntityAndElementConstructs())
                .ToImmutableList();

            var implementations = constructs
                .GroupBy(g => g.GetSubject(), (k, l) => (k, l.ToImmutableList()))
                .Select(g => new
                {
                    Subject = g.k,
                    Categories = g.Item2
                        .Where(i => i.HasVariant())
                        .GroupBy(i => i.GetVariant(), (m, n) => new
                        {
                            Variant = m,
                            Constructs = n
                        }),
                    General = g.Item2.Where(i => !i.HasVariant())
                })
                .ToList();

            var generalSubjects = implementations
                .Where(g => !g.Categories.Any())
                .Select(g => g.Subject)
                .ToImmutableHashSet();

            foreach (var implementation in implementations)
            {
                var subject = implementation.Subject;

                if (generalSubjects.Contains(subject))
                    yield return Object.Create(this, subject, implementation.General.ResolveConstructDictionary(subject, generalSubjects));
                else
                    foreach (var category in implementation.Categories)
                        yield return Object.Create(this, $"{category.Variant}{subject}", category.Constructs.Concat(implementation.General).ResolveConstructDictionary($"{category.Variant}{subject}", generalSubjects, category.Variant));
            }
        }

        private void AssignMembers()
        {
            foreach (var @object in Objects.Values)
            {
                @object.AssignMembers();
            }
        }

        private void AssignProperties()
        {
            var properties = Objects.Values
                .SelectMany(o => o.PropertyMembers.SelectMany(m => m.ResolveProperties()))
                .GroupBy(p => p.Object);

            foreach (var grouped in properties)
            {
                grouped.Key.Properties = grouped.Key.MutateProperties(grouped).ToImmutableList();

                grouped.Key.AssignMetaData();
            }
        }

        private void AssignCalls()
        {
            var calls = Objects.Values
                .SelectMany(o => o.MethodMembers.SelectMany(m => m.ResolveCalls()))
                .GroupBy(p => p.Object);

            foreach (var grouped in calls)
            {
                grouped.Key.Calls = grouped
                    .GroupBy(g => g.Caller)
                    .ToImmutableDictionary(g => g.Key, v => grouped.Key.MutateCalls(v).ToImmutableList());
            }
        }

        private IEnumerable<(string fullType, string type)> GetCached()
        {
            foreach (var @object in Objects.Values)
            {
                foreach (var cached in @object.PropertyMembers.OfType<CachedPropertyMember.CachedPropertyMerge>())
                {
                    yield return (cached.Type, cached.TypeName);
                }

                foreach (var hold in @object.MethodMembers.OfType<HoldMethodMember.HoldMethodMerge>())
                {
                    var parameter = hold.Parameters.Single();

                    yield return (parameter.fullType, parameter.type);
                }

                foreach (var release in @object.MethodMembers.OfType<ReleaseCachedMethodMember.ReleaseCachedMethodMerge>())
                {
                    var parameter = release.Parameters.Single();

                    yield return (parameter.fullType, parameter.type);
                }
            }
        }

        public string GetCode()
        {
            return $@"
using Core.Launcher;
using Core.Launcher.Domain;
using Launcher.Domain;

namespace Launcher;

public class Save : Save<Save>, ISave<Save>
{{{string.Join(@"
", GetStoreProperties().Select(p => $@"
    public {p}"))}

    public Save(string path = """") : base(path)
    {{{string.Join(@"
", GetStoreAssignments().Select(a => $@"
        {a};"))}
    }}

    public override void Flush()
    {{{string.Join(@"
", GetStoreFlushes().Select(f => $@"
        {f};"))}
    }}

    public static Pool<Save, TEntity> CreatePool<TEntity>(Save save, Schema schema, string? label = null, bool isSynchronized = false) where TEntity : IEntity<Save, TEntity>
    {{
        return new Pool<Save, TEntity>(save, schema, label, isSynchronized);
    }}
}}
";
        }

        private IEnumerable<string> GetStoreProperties()
        {
            foreach (var (name, _) in Pools)
            {
                yield return $"Pool<Save, {name}> {name}Store {{ get; }}";
            }

            foreach (var (fullType, type) in Caches)
            {
                yield return $"Cache<{fullType}> {type}Store {{ get; }}";
            }
        }

        private IEnumerable<string> GetStoreAssignments()
        {
            foreach (var (name, synchronized) in Pools)
            {
                yield return $"{name}Store = ReadPool<{name}>(this, {synchronized.ToString().ToLower()})";
            }

            foreach (var (fullType, type) in Caches)
            {
                yield return $"{type}Store = new Cache<{fullType}>()";
            }
        }

        private IEnumerable<string> GetStoreFlushes()
        {
            foreach (var (name, _) in Pools)
            {
                yield return $"{name}Store.Flush()";
            }
        }
    }
}
