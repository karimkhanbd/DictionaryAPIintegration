using Domain.Services;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Infrastructure.ExternalApi
{
    public class FreeDictionaryApiAdapter : IDictionaryService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public FreeDictionaryApiAdapter(HttpClient httpClient, IOptions<ApiConfiguration> apiConfig)
        {
            _httpClient = httpClient;

            _baseUrl = apiConfig.Value.BaseUrl;

            if (string.IsNullOrEmpty(_baseUrl))
            {
                throw new ArgumentNullException(nameof(apiConfig), "API BaseUrl configuration is missing or empty.");
            }
        }

        public async Task<WordDefinition?> GetDefinitionAsync(string word)
        {
            try
            {                
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                var responseStream = await _httpClient.GetStreamAsync($"{_baseUrl}{word}");
                
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
                .Where(p => !string.IsNullOrEmpty(p.Text) || !string.IsNullOrEmpty(p.Audio)) 
                .Select(p => new PhoneticInfo
                (
                    Text : p.Text ?? string.Empty,
                    AudioUrl: p.Audio ?? string.Empty,
                    SourceUrl: p.SourceUrl ?? string.Empty,
                    License: p.License != null ? new LicenseInfo
                    (
                         p.License.Name,
                         p.License.Url
                    ) : null
                ))
                .ToList() ?? new List<PhoneticInfo>();

            // 2. Map Meanings and Definitions (unchanged from previous fix)
            var definitions = apiResponse.Meanings?
                .SelectMany(m => m.Definitions.Select(d => new Definition
                    (
                    PartOfSpeech: m.PartOfSpeech ?? string.Empty,
                    Text: d.Definition,
                    Example: d.Example ?? string.Empty
                    )
                    {
                    Synonyms = d == m.Definitions.First() ? m.Synonyms ?? new List<string>() : new List<string>(),
                    Antonyms = d == m.Definitions.First() ? m.Antonyms ?? new List<string>() : new List<string>()
                    }
                ))
                .ToList() ?? new List<Definition>();

            // 3. Construct the final WordDefinition
            return new WordDefinition
              (
                Word: apiResponse.Word,
                License: apiResponse.License != null ? new LicenseInfo(apiResponse.License.Name, apiResponse.License.Url) : null
              )
             {
                Phonetics = phonetics,
                Definitions = definitions,
                SourceUrls = apiResponse.SourceUrls ?? new List<string>()
             };
        }
    }
}
