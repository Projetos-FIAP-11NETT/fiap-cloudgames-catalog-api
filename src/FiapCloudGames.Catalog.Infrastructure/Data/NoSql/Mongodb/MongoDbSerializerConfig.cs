using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson;

namespace FiapCloudGames.Catalog.Infrastructure.Data.NoSql.Mongodb;

public static class MongoDbSerializerConfig
{
    private static bool _registered = false;
    private static readonly Lock _lock = new();

    public static void Register()
    {
        lock (_lock)
        {
            if (_registered)
                return;

            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
            BsonSerializer.RegisterSerializer(new DecimalSerializer(BsonType.Decimal128));

            _registered = true;
        }
    }
}