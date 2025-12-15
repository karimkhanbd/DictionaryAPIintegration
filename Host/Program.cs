using Application.UseCases;
using Domain.Services;
using Infrastructure.ExternalApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


using IHost host =  Host.CreateDefaultBuilder(args)
     .ConfigureAppConfiguration((context, config) =>
     {
         // ensures the secret manager file is loaded into the configuration stack        
         config.AddUserSecrets(typeof(Program).Assembly, optional: true);
     })
    .ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Warning);
            logging.SetMinimumLevel(LogLevel.Information);
            logging.AddFilter("Microsoft", LogLevel.Error);
            logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Error);
        })
    .ConfigureServices((context, services) =>
    {

        services.AddOptions<ApiConfiguration>()
           .Bind(context.Configuration.GetSection("Api"))
           .Validate(cfg => !string.IsNullOrWhiteSpace(cfg.BaseUrl),
               "Api:BaseUrl is not configured. Set it via user-secrets: dotnet user-secrets set \"Api:BaseUrl\" \"...\"")
           .ValidateOnStart();

        services.AddHttpClient<IDictionaryService, FreeDictionaryApiAdapter>((serviceProvider, client) =>
        {
            var apiConfig = serviceProvider.GetRequiredService<IOptions<ApiConfiguration>>().Value;          
            client.BaseAddress = new Uri(apiConfig.BaseUrl);
        });
        
        services.AddTransient<GetWordDefinitionQuery>();


    }).Build();

var configuration = host.Services.GetRequiredService<IConfiguration>();

await RunApp(host.Services);
Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();


static async Task RunApp(IServiceProvider services)
{

    using var scope = services.CreateScope();
    var query = scope.ServiceProvider.GetRequiredService<GetWordDefinitionQuery>();

    Console.OutputEncoding = System.Text.Encoding.UTF8;

    Console.Title = "Dictionary App";

    while (true)
    {

        Console.Write("Enter a word to define (or type 'exit' to quit):");
        var word = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(word))
        {
            Console.WriteLine("No word entered. Exiting.");
            return;
        }

        if (word.Equals("exit", StringComparison.OrdinalIgnoreCase))
        {
            break;
        }

        try
        {
            Console.WriteLine($"\nSearching for: **{word}**...\n");

            var result = await query.ExecuteAsync(word);

            Console.WriteLine($"word: {result.Word}");

            if (result.Phonetics.Any())
            {
                Console.WriteLine("\n Phonetics :");

                foreach (var pInfo in result.Phonetics)
                {
                    Console.WriteLine($"\n    Text: {pInfo.Text ?? "N/A"}");

                    if (!string.IsNullOrEmpty(pInfo.AudioUrl))
                        Console.WriteLine($"    Audio URL: {pInfo.AudioUrl}");
                    if (!string.IsNullOrEmpty(pInfo.SourceUrl))
                        Console.WriteLine($"    Source URL: {pInfo.SourceUrl}");

                    Console.WriteLine("    License :");

                    if (pInfo.License != null)
                    {
                        Console.WriteLine($"        Name: {pInfo.License.Name} ");
                        Console.WriteLine($"        Url: {pInfo.License.Url}");
                    }
                }
            }

            var definitionsByPart = result.Definitions
                .GroupBy(d => d.PartOfSpeech);

            foreach (var group in definitionsByPart)
            {
                Console.WriteLine($"\n Part of Speech: {group.Key}");

                foreach (var definition in group)
                {

                    Console.WriteLine($"  Definition:\"{definition.Text} \"");

                    if (!string.IsNullOrEmpty(definition.Example))
                    {
                        Console.WriteLine($"       Example: \"{definition.Example}\"");
                    }

                    if (definition.Synonyms != null && definition.Synonyms.Any())
                    {
                        Console.WriteLine($"       Synonyms: {string.Join(", ", definition.Synonyms.Take(5))}");
                    }

                    if (definition.Antonyms != null && definition.Antonyms.Any())
                    {
                        Console.WriteLine($"       Antonyms: {string.Join(", ", definition.Antonyms.Take(5))}");
                    }
                }
            }
            Console.WriteLine("\n---------------------------------------------------");


        }
        catch (KeyNotFoundException ex)
        {
            Console.WriteLine($"\nDefinition Not Found: {ex.Message}");
        }
        catch (ApplicationException ex)
        {
            Console.WriteLine($"\nCommunication Error: The application could not reach the dictionary API.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nAn unexpected error occurred: {ex.Message}");
        }
    }
}
