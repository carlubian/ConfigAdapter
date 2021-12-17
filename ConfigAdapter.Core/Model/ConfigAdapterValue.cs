using ConfigAdapter.Core.Exceptions;

namespace ConfigAdapter.Core.Model;

public abstract record ConfigAdapterValue
{
    public abstract string TypeHint { get; }

    public record StringValue(string Value) : ConfigAdapterValue()
    {
        public override string TypeHint => "string";

        public static implicit operator StringValue(string s) => new(s);
        public static implicit operator string(StringValue s) => s.Value;
    }

    public record ArrayValue(List<string> Value) : ConfigAdapterValue()
    {
        public override string TypeHint => "array";

        public static implicit operator ArrayValue(List<string> s) => new(s);
        public static implicit operator ArrayValue(string[] s) => new(s);
        public static implicit operator List<string>(ArrayValue s) => s.Value;
        public static implicit operator string[](ArrayValue s) => s.Value.ToArray();
    }

    public record EmptyValue() : ConfigAdapterValue() 
    {
        public override string TypeHint => "empty";
    }

    // Implicit conversions from outside

    public static implicit operator ConfigAdapterValue(string v)
    {
        return new StringValue(v);
    }

    public static implicit operator ConfigAdapterValue(List<string> v)
    {
        return new ArrayValue(v);
    }

    // Implicit conversions to outside

    public static implicit operator string(ConfigAdapterValue c)
    {
        if (c is StringValue s)
            return s.Value;

        throw new ValueMismatchException();
    }

    public static implicit operator List<string>(ConfigAdapterValue c)
    {
        if (c is ArrayValue a)
            return a.Value;

        throw new ValueMismatchException();
    }
}
