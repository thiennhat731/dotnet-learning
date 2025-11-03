using CollabDoc.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace CollabDoc.Infrastructure.Mappings
{
    public static class DocumentMap
    {
        public static void RegisterClassMap()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(Document)))
            {
                BsonClassMap.RegisterClassMap<Document>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdMember(x => x.Id)
                      .SetIdGenerator(MongoDB.Bson.Serialization.IdGenerators.StringObjectIdGenerator.Instance)
                      .SetSerializer(new StringSerializer(BsonType.ObjectId));
                });
            }
        }
    }
}
