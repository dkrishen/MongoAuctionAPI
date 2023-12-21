using Microsoft.Extensions.Options;
using MongoAuction.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;

namespace MongoAuction.Services;

public class MongoDBService
{
    private readonly IMongoCollection<User> _userCollection;

    public MongoDBService(IOptions<MongoDBSettings> mongoDBSettings)
    {
        MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionURI);
        IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
        _userCollection = database.GetCollection<User>(mongoDBSettings.Value.CollectionName);
    }

}