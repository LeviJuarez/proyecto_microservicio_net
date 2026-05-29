
using SearchService;
using Polly;
using Polly.Extensions.Http;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(async () =>
{
        
    try
    {
        await DbInitializer.InitializeAsync(app);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error initializing database: {ex.Message}");
    }
});

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetPolicy()
    => HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));

