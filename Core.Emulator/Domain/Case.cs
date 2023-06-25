namespace Core.Emulator.Domain
{
    public class Case
    {
        public string Method { get; }

        public string Subject { get; }

        public string Property { get; }

        public object Value { get; }

        public Case Nested { get; set; }

        public Case(string method, string subject, string property, object value)
        {
            Method = method;
            Subject = subject;
            Property = property;
            Value = value;
        }

        public Case(Case @case, Case nested)
        {
            Method = @case.Method;
            Subject = @case.Subject;
            Property = @case.Property;
            Value = @case.Value;
            Nested = nested;
        }

        public override string ToString()
        {
            return $"{Method}";
        }
    }
}
