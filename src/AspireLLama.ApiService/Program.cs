using AspireLLama.ApiService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.AddOllamaApiClient("chat").AddChatClient();

builder.Services.AddHttpClient();

var googleApiKey = builder.Configuration.GetValue<string>("GOOGLE_API_KEY")
?? throw new ArgumentNullException("GOOGLE_API_KEY");

builder.Services.AddScoped<WeatherService>(x => new(x.GetRequiredService<IHttpClientFactory>(), googleApiKey));


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("/chat",
    async (IChatClient chatClient, [FromBody] ChatRequest question, CancellationToken cancellationToken) =>
    {
        var response = await chatClient.GetResponseAsync(
            question.Question,
            cancellationToken: cancellationToken);
        return Results.Ok(response.ToString());
    }).WithName("ChatWithLlama");

app.MapGet("/weatherforecast",
        (string cityName, WeatherService service) =>
            service.GetTemperature(cityName)
    )
    .WithName("GetWeatherForecast");

app.MapDefaultEndpoints();

app.Run();

record ChatRequest(string Question);