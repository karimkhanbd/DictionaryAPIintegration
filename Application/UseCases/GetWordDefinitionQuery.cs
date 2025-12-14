using Domain.Entities;
using Domain.Services;

namespace Application.UseCases
{
    public class GetWordDefinitionQuery
    {
        private readonly IDictionaryService _dictionaryService;
       
        public GetWordDefinitionQuery(IDictionaryService dictionaryService)
        {
            _dictionaryService = dictionaryService;
        }

        public async Task<WordDefinition> ExecuteAsync(string word)
        {
           
            if (string.IsNullOrWhiteSpace(word))
            {
                throw new ArgumentException("Word cannot be empty.", nameof(word));
            }            
            var definition = await _dictionaryService.GetDefinitionAsync(word);

            if (definition == null)
            {               
                throw new KeyNotFoundException($"No definition found for word: '{word}'");
            }

            return definition;
        }
    }
}
