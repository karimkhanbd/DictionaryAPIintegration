namespace Infrastructure.ExternalApi
{
    public class ApiPhonetic
    {
        public string Text { get; set; }
        public string Audio { get; set; } // URL to audio file
        public string SourceUrl { get; set; }
        public ApiLicense License { get; set; } // Phonetic-level license
    }
}
