using System;
using MongoDB.Bson.Serialization.Attributes;

namespace AppCoreNet.Data.MongoDB.DAO;

public class TestEntity
{
    [BsonId]
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? ChangeToken { get; set; }
}