    public record ApiDictionaryResponse    (
         string Word ,
         string? Phonetic,
         List<ApiPhonetic>? Phonetics ,
         List<ApiMeaning>? Meanings,
         ApiLicense? License,
         List<string>? SourceUrls 
    );
