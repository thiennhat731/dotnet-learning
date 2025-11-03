using CollabDoc.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace CollabDoc.Infrastructure.Mappings
{
    public static class UserMap
    {
        public static void RegisterClassMap()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(User)))
            {
                BsonClassMap.RegisterClassMap<User>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdMember(x => x.Id)
                      .SetIdGenerator(MongoDB.Bson.Serialization.IdGenerators.StringObjectIdGenerator.Instance)
                      .SetSerializer(new StringSerializer(BsonType.ObjectId));
                    cm.MapMember(x => x.Email).SetIsRequired(true);
                    cm.MapMember(x => x.RefreshToken).SetIsRequired(false);

                });
            }
        }
    }
}
