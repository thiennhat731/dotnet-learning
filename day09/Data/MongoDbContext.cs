using day09.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace day09.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration config)
    {
        var connection = config["MongoDB:ConnectionString"];
        var dbName = config["MongoDB:DatabaseName"];
        var client = new MongoClient(connection);
        _database = client.GetDatabase(dbName);
    }

    public IMongoCollection<Product> Products =>
        _database.GetCollection<Product>("Products");
}
