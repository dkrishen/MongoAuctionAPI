using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoAuction.Models;

public class User :IDisposable
{
    [BsonRepresentation(BsonType.String)]
    public string Username { get; set; }

    [BsonId]
        public string FakeJWT { get; set; }

    [BsonElement("Lots")]
    public List<Lot> Lots { get; set; }

    public void Dispose()
    {
        Lots = null;
        GC.SuppressFinalize(this);
    }
}
