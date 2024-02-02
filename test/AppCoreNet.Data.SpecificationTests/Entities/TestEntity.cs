using System;

namespace AppCoreNet.Data.Entities;

public class TestEntity : IEntity<Guid>, IHasChangeToken
{
    public Guid Id { get; set; }

    object IEntity.Id => Id;

    public string? Name { get; set; }

    public string? ChangeToken { get; set; }
}