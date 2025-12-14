
namespace Domain.Services
{
    public interface IDictionaryService
    {
        Task<WordDefinition?> GetDefinitionAsync(string word);
    }
}
