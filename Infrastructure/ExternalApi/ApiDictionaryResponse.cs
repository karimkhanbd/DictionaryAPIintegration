    public record ApiDictionaryResponse    (
         string Word ,
         List<ApiPhonetic>? Phonetics ,
         List<ApiMeaning>? Meanings,
         ApiLicense? License,
         List<string>? SourceUrls 
    );
