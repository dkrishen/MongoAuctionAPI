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

    #region users

    public async Task<string> CreateUserAsync(string username)
    {
        await _userCollection.InsertOneAsync(new User 
            { Username = username, 
              FakeJWT = Guid.NewGuid().ToString(),
              Lots = new List<Lot>() 
            })
            .ConfigureAwait(false);
        return await GetFakeJWTAsync(username)
            .ConfigureAwait(false);
    }

    public async Task<List<string>> GetUsersAsync()
    {
        return (await _userCollection.FindAsync(new BsonDocument())
            .ConfigureAwait(false))
            .ToList()
            .Select(u => u.Username)
            .ToList();
    }

    public async Task<string> GetFakeJWTAsync(string username)
    {
        FilterDefinition<User> filter = Builders<User>.Filter.Eq("Username", username);
        return (await _userCollection.FindAsync(filter)
            .ConfigureAwait(false))
            .FirstOrDefault().FakeJWT;
    }

    public async Task<User> ParseTokenAsync(string token)
    {
        FilterDefinition<User> filter = Builders<User>.Filter.Eq("FakeJWT", token);
        return (await _userCollection.FindAsync(filter)
            .ConfigureAwait(false))
            .FirstOrDefault();
    }

    public async Task RemoveUserAsync(string username)
    {
        FilterDefinition<User> filter = Builders<User>.Filter.Eq("Username", username);
        await _userCollection.DeleteOneAsync(filter)
            .ConfigureAwait(false);
    }

    #endregion

}