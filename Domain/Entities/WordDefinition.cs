namespace Domain.Entities
{
    public class WordDefinition
    {
        public string Word { get; set; }       
        public List<PhoneticInfo> Phonetics { get; set; } = new List<PhoneticInfo>();
        public List<Definition> Definitions { get; set; } = new List<Definition>();
        public LicenseInfo License { get; set; }
        public List<string> SourceUrls { get; set; } = new List<string>();
    }
}
