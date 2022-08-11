namespace ConfigAdapter.Core.Model;

public record ConfigAdapterSection
{
    public string Name { get; set; } = string.Empty;
    public IList<ConfigAdapterSetting> Settings { get; set; } = Array.Empty<ConfigAdapterSetting>();
    public IList<ConfigAdapterSection> Nested { get; set; } = Array.Empty<ConfigAdapterSection>();
}
