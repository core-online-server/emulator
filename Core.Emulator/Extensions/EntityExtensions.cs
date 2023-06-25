using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System;
using Core.Generator.Extensions;

namespace Core.Emulator.Extensions
{
    public static class EntityExtensions
    {
        public static ImmutableDictionary<ISymbol, ImmutableDictionary<ISymbol, string>> ResolveConstructDictionary(this IEnumerable<INamedTypeSymbol> constructs, string name, ImmutableHashSet<string> generalSubjects, string variant = null)
        {
            return constructs.ToImmutableDictionary(i => i, i => i.TypeParameters.ToImmutableDictionary(p => p, p => p.ResolveEntityOrElementParameter(name, variant, generalSubjects), SymbolEqualityComparer.Default), SymbolEqualityComparer.Default);
        }

        public static string ResolveEntityOrElementParameter(this ITypeParameterSymbol parameter, string name, string parentVariant, ImmutableHashSet<string> generalSubjects)
        {
            var constraints = parameter.ConstraintTypes
                .OfType<INamedTypeSymbol>();

            if (!constraints.Any())
                throw new InvalidOperationException($"Missing object constraint on {parameter} of {parameter.ContainingSymbol}");

            var @interface = parameter.ConstraintTypes
                .OfType<INamedTypeSymbol>()
                .Single();

            if (@interface.HasEntityAttribute() || @interface.HasElementAttribute()) return @interface.ResolveEntityOrElementParameter(parentVariant, generalSubjects);

            var nested = (ITypeParameterSymbol)@interface.TypeArguments.SingleOrDefault();

            if (nested == null) throw new InvalidOperationException($"Missing entity attribute on {@interface}.");

            var resolved = nested.ResolveEntityOrElementParameter(name, parentVariant, generalSubjects);

            if (@interface.IsProducerConsumerCollection())
            {
                if (parameter.Name.EndsWith("ConcurrentQueue")) return $"ConcurrentQueue<{resolved}>";
                else if (parameter.Name.EndsWith("ConcurrentStack")) return $"ConcurrentStack<{resolved}>";
                else if (parameter.Name.EndsWith("Queue")) return $"Queue<{resolved}>";
                else if (parameter.Name.EndsWith("Stack")) return $"Stack<{resolved}>";
            }

            if (@interface.IsCollection())
            {
                if (parameter.Name.EndsWith("Collection")) return $"List<{resolved}>";
            }

            if (@interface.IsArray())
            {
                if (parameter.Name.EndsWith("Array")) return $"Array<Save, {resolved}>";
            }

            throw new InvalidOperationException("Invalid entity or element parameter.");
        }

        public static string ResolveEntityOrElementParameter(this INamedTypeSymbol @interface, string parentVariant, ImmutableHashSet<string> generalSubjects)
        {
            var subject = @interface.GetSubject();

            var childVariant = @interface.GetVariant();

            var result = generalSubjects.Contains(subject) ? subject : string.IsNullOrEmpty(childVariant) ? $"{parentVariant}{subject}" : $"{childVariant}{subject}";

            return result;
        }

        public static string GetSubject(this INamedTypeSymbol symbol)
        {
            return symbol.HasElementAttribute() ? symbol.GetElementAttribute().GetSubject() : symbol.GetEntityAttribute().GetSubject();
        }

        public static string GetSubject(this AttributeData attribute)
        {
            if (attribute.ConstructorArguments.Length < 2)
                return (string)attribute.ConstructorArguments[0].Value;

            return (string)attribute.ConstructorArguments[1].Value;
        }

        public static string GetVariant(this INamedTypeSymbol symbol)
        {
            return symbol.HasElementAttribute() ? symbol.GetElementAttribute().GetVariant() : symbol.GetEntityAttribute().GetVariant();
        }

        public static string GetVariant(this AttributeData attribute)
        {
            if (attribute.ConstructorArguments.Length < 2)
                return string.Empty;

            return (string)attribute.ConstructorArguments[0].Value;
        }

        public static bool IsSynchronizedAttribute(this AttributeData attribute)
        {
            return attribute.IsAttribute("SynchronizedAttribute");
        }

        public static bool HasVariant(this INamedTypeSymbol symbol)
        {
            return !string.IsNullOrEmpty(symbol.GetVariant());
        }

        public static bool IsProducerConsumerCollection(this INamedTypeSymbol symbol)
        {
            return symbol.Name == "IProducerConsumerCollection";
        }

        public static bool IsCollection(this INamedTypeSymbol symbol)
        {
            return symbol.Name == "ICollection";
        }

        public static bool IsArray(this INamedTypeSymbol symbol)
        {
            return symbol.Name == "IReadOnlyList";
        }
    }
}
