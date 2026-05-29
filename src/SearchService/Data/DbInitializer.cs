using System.Text.Json;
using MongoDB.Entities;
using MongoDB.Driver;

namespace SearchService;

public class DbInitializer
{
    public static async Task InitializeAsync(WebApplication app)
    {
        var db = await DB.InitAsync("SearchDb", MongoClientSettings
            .FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

        await db.Index<Item>()
            .Key(x => x.Make, KeyType.Text)
            .Key(x => x.Model, KeyType.Text)
            .Key(x => x.Color, KeyType.Text)
            .CreateAsync();

        var count = await db.CountAsync<Item>();
        
        using var scope = app.Services.CreateScope();
        var httpClient = scope.ServiceProvider.GetRequiredService<AuctionSvcHttpClient>();
        var items = await httpClient.GetItemsForSearchDb();
        Console.WriteLine(items.Count + " returned from auction service");
        if (items.Count > 0)
        {
            foreach (var item in items)
            {
                await db.SaveAsync(item);
            }
        }
    }
}