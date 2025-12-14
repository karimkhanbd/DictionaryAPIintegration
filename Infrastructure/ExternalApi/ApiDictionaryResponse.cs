namespace Infrastructure.ExternalApi
{
    public class ApiDictionaryResponse
    {
        public string Word { get; set; }
        public List<ApiPhonetic> Phonetics { get; set; } 
        public List<ApiMeaning> Meanings { get; set; }      
        public ApiLicense License { get; set; } 
        public List<string> SourceUrls { get; set; }


    }


}
