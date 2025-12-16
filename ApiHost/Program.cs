using Application.UseCases;
using Domain.Services;
using Infrastructure.ExternalApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;



var builder = WebApplication.CreateBuilder(args);


builder.Configuration.AddUserSecrets(typeof(Program).Assembly, optional: true);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddFilter("Microsoft", LogLevel.Warning); // Less noisy logging

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOptions<ApiConfiguration>()
    .Bind(builder.Configuration.GetSection("Api"))
    .Validate(cfg => !string.IsNullOrWhiteSpace(cfg.BaseUrl), "Api:BaseUrl is not configured.")
    .ValidateOnStart();

builder.Services.AddHttpClient<IDictionaryService, FreeDictionaryApiAdapter>((serviceProvider, client) =>
{
    var apiConfig = serviceProvider.GetRequiredService<IOptions<ApiConfiguration>>().Value;
    client.BaseAddress = new Uri(apiConfig.BaseUrl);
});

builder.Services.AddTransient<GetWordDefinitionQuery>();


var app = builder.Build();



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); 


app.MapGet("/api/v1/dictionary/{word}",
    async ([FromRoute] string word, GetWordDefinitionQuery query, ILogger<Program> logger) =>
    {
        try
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return Results.BadRequest(new { error = "Word parameter cannot be empty." });
            }


            var result = await query.ExecuteAsync(word);

            if (result == null)
            {

                return Results.NotFound(new { error = $"Definition not found for word: {word}" });
            }


            return Results.Ok(result);
        }
        catch (ApplicationException ex)
        {
            logger.LogError(ex, "External API communication failed during lookup for {Word}.", word);

            return Results.Json(
                new { error = "External dictionary service is temporarily unavailable." },
                statusCode: StatusCodes.Status503ServiceUnavailable
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected server error occurred.");

            return Results.Json(
                new { error = "An unexpected server error occurred." },
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    })
.WithName("GetDefinition")
.Produces<WordDefinition>(StatusCodes.Status200OK)
.ProducesProblem(StatusCodes.Status404NotFound)
.ProducesProblem(StatusCodes.Status503ServiceUnavailable);

app.Run();