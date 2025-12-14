using Domain.Entities;
using Domain.Services;
using System.Text.Json;

namespace Infrastructure.ExternalApi
{
    public class FreeDictionaryApiAdapter : IDictionaryService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://api.dictionaryapi.dev/api/v2/entries/en/";

        public FreeDictionaryApiAdapter(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<WordDefinition> GetDefinitionAsync(string word)
        {
            try
            {                
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                var responseStream = await _httpClient.GetStreamAsync($"{BaseUrl}{word}");
                
                var apiResponse = await JsonSerializer.DeserializeAsync<List<ApiDictionaryResponse>>(
                    responseStream, options);

                if (apiResponse == null || !apiResponse.Any())
                {
                    return null;
                }

                return MapToDomainEntity(apiResponse.First());
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
               return null;
            }
            catch (HttpRequestException ex)
            {
               throw new ApplicationException($"Error communicating with Dictionary API: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {                
                throw new ApplicationException($"Error parsing API response: {ex.Message}", ex);
            }
        }

        private static WordDefinition MapToDomainEntity(ApiDictionaryResponse apiResponse)
        {
            // 1. Map all phonetic entries (including audio, source, and license)
            var phonetics = apiResponse.Phonetics?
                .Where(p => !string.IsNullOrEmpty(p.Text) || !string.IsNullOrEmpty(p.Audio)) // Only map useful phonetic entries
                .Select(p => new PhoneticInfo
                {
                    Text = p.Text,
                    AudioUrl = p.Audio,
                    SourceUrl = p.SourceUrl,
                    License = p.License != null ? new LicenseInfo
                    {
                        Name = p.License.Name,
                        Url = p.License.Url
                    } : null
                })
                .ToList() ?? new List<PhoneticInfo>();

            // 2. Map Meanings and Definitions (unchanged from previous fix)
            var definitions = apiResponse.Meanings?
                .SelectMany(m => m.Definitions.Select(d => new Definition
                {
                    PartOfSpeech = m.PartOfSpeech,
                    Text = d.Definition,
                    Example = d.Example,
                    Synonyms = d == m.Definitions.First() ? m.Synonyms ?? new List<string>() : new List<string>(),
                    Antonyms = d == m.Definitions.First() ? m.Antonyms ?? new List<string>() : new List<string>()
                }))
                .ToList() ?? new List<Definition>();

            // 3. Construct the final WordDefinition
            return new WordDefinition
            {
                Word = apiResponse.Word,
                Phonetics = phonetics,
                Definitions = definitions,
                License = apiResponse.License != null ? new LicenseInfo 
                {
                    Name = apiResponse.License.Name,
                    Url = apiResponse.License.Url
                } : null,
                SourceUrls = apiResponse.SourceUrls ?? new List<string>() 
            };
        }
    }
}
