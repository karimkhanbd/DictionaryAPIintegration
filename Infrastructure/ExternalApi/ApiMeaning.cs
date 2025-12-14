  public record ApiMeaning (
         string PartOfSpeech ,
         List<ApiDefinition> Definitions,
         List<string>? Synonyms ,
         List<string>? Antonyms 
      );
