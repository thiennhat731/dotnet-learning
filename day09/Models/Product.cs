using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace day09.Models;

public class Product
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = "";

    [BsonElement("price")]
    public double Price { get; set; }

    [BsonElement("category")]
    public string Category { get; set; } = "";
}
