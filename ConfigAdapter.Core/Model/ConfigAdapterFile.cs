namespace ConfigAdapter.Core.Model;

public record ConfigAdapterFile
{
    public string FileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public IList<ConfigAdapterSection> Sections { get; set; } = Array.Empty<ConfigAdapterSection>();
}
