namespace Infrastructure.ExternalApi
{
    public class ApiMeaning
    {
        public string PartOfSpeech { get; set; }
        public List<ApiDefinition> Definitions { get; set; }
        public List<string> Synonyms { get; set; }
        public List<string> Antonyms { get; set; }
    }
}
