public record WordDefinition(
     string Word,
     LicenseInfo? License
  )
{
    public List<PhoneticInfo> Phonetics { get; init; } = new List<PhoneticInfo>();
    public List<Definition> Definitions { get; init; } = new List<Definition>();
    public List<string> SourceUrls { get; init; } = new List<string>();
}

