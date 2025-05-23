// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using MongoDB.Bson.Serialization.Attributes;

namespace AppCoreNet.Data.MongoDB.DAO;

public class TestEntity2
{
    [BsonId]
    public ComplexId Id { get; set; } = new() { Id = Guid.Empty, Version = 0 };

    public string? Name { get; set; }

    public string? ChangeToken { get; set; }
}