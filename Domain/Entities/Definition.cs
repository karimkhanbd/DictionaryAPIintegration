public record Definition(
      string PartOfSpeech,
      string Text,
      string Example)
{
    public IEnumerable<string> Synonyms { get; init; } =new List<string>();
    public IEnumerable<string> Antonyms { get; init; } =new List<string>();
}