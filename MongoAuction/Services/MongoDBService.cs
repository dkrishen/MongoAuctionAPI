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
    #region lots

    public async Task<IEnumerable<string>> GetLotsAsync()
    {
        return (await _userCollection.FindAsync(new BsonDocument())
            .ConfigureAwait(false))
            ?.ToList()
            ?.SelectMany(u => u.Lots)
            ?.Where(l => l.IsActive)
            ?.Select(l => l.Title)
            ?.ToList() ?? new List<string>();
    }

    public async Task<IEnumerable<string>> GetLotsByCategoryAsync(string category)
    {
        return (await _userCollection.FindAsync(new BsonDocument())
            .ConfigureAwait(false))
            .ToList()
            .SelectMany(u => u.Lots)
            .Where(l => l.IsActive && l.ItemCategory == category)
            .Select(l => l.Title)
            .ToList();
    }

    public async Task<IEnumerable<string>> GetLotsByUserAsync(string username)
    {
        FilterDefinition<User> filter = Builders<User>.Filter.Eq("Username", username);
        return (await _userCollection.FindAsync(filter)
            .ConfigureAwait(false))
            .ToList()
            .SelectMany(u => u.Lots)
            .Where(l => l.IsActive)
            .Select(l => l.Title)
            .ToList();
    }

    public async Task<IEnumerable<string>> GetLotsByTokenAsync(string token)
    {
        FilterDefinition<User> filter = Builders<User>.Filter.Eq("FakeJWT", token);
        return (await _userCollection.FindAsync(filter)
            .ConfigureAwait(false))
            .ToList()
            .SelectMany(u => u.Lots)
            .Where(l => l.IsActive)
            .Select(l => l.Title)
            .ToList();
    }

    public async Task<IEnumerable<string>> GetLotsByBidderTokenAsync(string token)
    {
        var user = await ParseTokenAsync(token)
            .ConfigureAwait(false);

        var lots = (await _userCollection.FindAsync(new BsonDocument())
            .ConfigureAwait(false))
            ?.ToList()
            ?.SelectMany(u => u.Lots)
            ?.Where(l => l.IsActive 
                && l.LastBidderName == user.Username)
            .Select(l => l.Title);

        return lots;
    }

    public async Task<Lot> GetLotByTitleAsync(string title)
    {
        return (await _userCollection.FindAsync(new BsonDocument())
            .ConfigureAwait(false))
            .ToList()
            .SelectMany(u => u.Lots)
            .Where(l => l.IsActive)
            .FirstOrDefault(l => l.Title == title);
    }

    public async Task CreateLotAsync(LotInputDto lot, string token)
    {
        var user = await ParseTokenAsync(token)
            .ConfigureAwait(false);
        var newLot = CreateLot(lot, user.Username);

        FilterDefinition<User> filter = Builders<User>.Filter.Eq("FakeJWT", token);
        UpdateDefinition<User> update = Builders<User>.Update.AddToSet<Lot>("Lots", newLot);

        await _userCollection.UpdateOneAsync(filter, update)
            .ConfigureAwait(false);
    }

    public async Task CreateLotAsync(Lot lot, string token)
    {
        FilterDefinition<User> filter = Builders<User>.Filter.Eq("FakeJWT", token);
        UpdateDefinition<User> update = Builders<User>.Update.AddToSet<Lot>("Lots", lot);

        await _userCollection.UpdateOneAsync(filter, update)
            .ConfigureAwait(false);
    }

    public async Task RemoveLotAsync(string title, string token)
    {
        var lot = await GetLotByTitleAsync(title)
            .ConfigureAwait(false);

        FilterDefinition<User> filter = Builders<User>.Filter.Eq("FakeJWT", token);
        UpdateDefinition<User> update = Builders<User>.Update.Pull<Lot>("Lots", lot);

        await _userCollection.UpdateOneAsync(filter, update)
            .ConfigureAwait(false);
    }

    public async Task<Lot> BidLotAsync(BidParams bidParams, string token)
    {
        var lot = await GetLotByTitleAsync(bidParams.LotTitle)
            .ConfigureAwait(false);

        var lotOwnerToken = await GetFakeJWTAsync(lot.LotOwnerName)
            .ConfigureAwait(false);

        var user = await ParseTokenAsync(token)
            .ConfigureAwait(false);


        var newLot = BidLotAsync(lot, bidParams.BidAmount, user.Username);

        await RemoveLotAsync(bidParams.LotTitle, lotOwnerToken)
            .ConfigureAwait(false);
        await CreateLotAsync(newLot, lotOwnerToken)
            .ConfigureAwait(false);

        return await GetLotByTitleAsync(bidParams.LotTitle)
            .ConfigureAwait(false);
    }

    private Lot BidLotAsync(Lot lot, double bidAmount, string username)
    {
        if (lot.CurrentCost * lot.CostStep > bidAmount)
            return lot;

        lot.CurrentCost += bidAmount;
        lot.LastBidderName = username;

        if (lot.CurrentCost >= lot.EndCost)
            lot.IsActive = false;

        return lot;
    }

    private Lot CreateLot(LotInputDto lotDto, string username)
    {
        var props = JsonSerializer.Deserialize<Dictionary<string, string>>(lotDto.AdditionalItemProperties);

        return new Lot
        {
            Id = Guid.NewGuid().ToString(),
            Title = lotDto.Title,
            StartTime = TimeOnly.ParseExact(lotDto.StartTime, "HH:mm"),
            StartDate = DateOnly.ParseExact(lotDto.StartDate, "yyyy-MM-dd"),
            Period = lotDto.Period,
            CurrentCost = lotDto.StartCost,
            LastBidderName = null,
            LotOwnerName = username,
            StartCost = lotDto.StartCost,
            EndCost = lotDto.EndCost,
            CostStep = lotDto.CostStep,
            ItemCategory = lotDto.ItemCategory,
            IsActive = true,
            ItemProperties = props ?? new Dictionary<string, string>(),
        };
    }

    #endregion

}