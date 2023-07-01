using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Core.Emulator.Domain.Members.Methods;

namespace Core.Emulator.Domain
{
    public class Call
    {
        public Object Object { get; set; }

        public MethodMember.MethodMerge MethodMerge { get; set; }

        public string Caller { get; set; }

        public Case Case { get; set; }

        public (string parameter, int value) Also { get; set; }

        public double Priority { get; }

        public string Name { get; }

        public string FullName { get; }

        public ImmutableArray<(string @ref, string fullType, string type, string name)> Parameters { get; }

        public bool Return { get; }

        public Call(string fullName, IEnumerable<(string @ref, string fullType, string type, string name)> parameters, bool @return = false)
        {
            FullName = fullName;
            Parameters = parameters.ToImmutableArray();
            Return = @return;
        }

        public Call(double priority, string name, string fullName, IEnumerable<(string @ref, string fullType, string type, string name)> parameters, bool @return = false) : this(fullName, parameters, @return)
        {
            Priority = priority;
            Name = name;
        }

        public string GetCode()
        {
            return $"{(Return ? "return " : string.Empty)}{FullName}({string.Join(", ", Parameters.Select(p => p.name == Also.parameter ? $"{GetRef(p.@ref)}{Also.value}" : $"{GetRef(p.@ref)}{p.name}"))});";

            static string GetRef(string value)
            {
                return string.IsNullOrEmpty(value) ? string.Empty : $"{value} ";
            }
        }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
