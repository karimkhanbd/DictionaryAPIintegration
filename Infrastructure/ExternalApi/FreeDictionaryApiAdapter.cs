using Domain.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;

namespace Infrastructure.ExternalApi
{
    public class FreeDictionaryApiAdapter : IDictionaryService
    {  
        private readonly ILogger<FreeDictionaryApiAdapter> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public FreeDictionaryApiAdapter(HttpClient httpClient, IOptions<ApiConfiguration> apiConfig,ILogger<FreeDictionaryApiAdapter> logger)
        {
            _logger = logger;
            _httpClient = httpClient;
            _baseUrl = apiConfig.Value.BaseUrl;

            if (string.IsNullOrEmpty(_baseUrl))
            {
                _logger.LogCritical("API BaseUrl is missing. This should have been caught by startup validation.");
                throw new ArgumentNullException(nameof(apiConfig), "API BaseUrl configuration is missing or empty.");
            }
        }

        public async Task<WordDefinition?> GetDefinitionAsync(string word)
        {

            if(string.IsNullOrWhiteSpace(word))
            {
                _logger.LogWarning("Attempted to query API with null or empty word.");
                throw new ArgumentException("Word cannot be null or empty.", nameof(word));
            }

             string fullRequestUrl = $"{_baseUrl}{word.ToLowerInvariant()}";
            _logger.LogInformation("Sending API request for word: {Word} at URL: {Url}", word, fullRequestUrl);

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
                    _logger.LogWarning("API returned an empty/null response for word: {Word}", word);
                    return null;
                }
                _logger.LogInformation("Successfully retrieved and mapped definition for word: {Word}", word);

                return MapToDomainEntity(apiResponse.First());
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("API returned 404 Not Found for word: {Word}", word);
                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error communicating with Dictionary API for word: {Word}. Status code: {StatusCode}",
                                 word, ex.StatusCode ?? HttpStatusCode.Unused);
                throw new ApplicationException($"Error communicating with Dictionary API: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error parsing JSON response for word: {Word}", word);
                throw new ApplicationException($"Error parsing API response: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during API processing for word: {Word}", word);
                throw;
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
                Phonetic: apiResponse.Phonetic ?? string.Empty
              )
             {
                Phonetics = phonetics,
                Definitions = definitions,
                SourceUrls = apiResponse.SourceUrls ?? new List<string>()
             };
        }
    }
}
