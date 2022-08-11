namespace ConfigAdapter.Core.Model;

public record ConfigAdapterSetting
{
    public string Name { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public ConfigAdapterValue Value { get; set; } = new ConfigAdapterValue.EmptyValue();

    public void Deconstruct(out string name, out ConfigAdapterValue value, out string? comment)
    {
        name = Name;
        comment = Comment;
        value = Value;
    }
}
