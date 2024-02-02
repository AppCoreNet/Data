using System;

namespace AppCoreNet.Data.Entities;

public class TestEntity2 : IEntity<ComplexId>, IHasChangeTokenEx
{
    public ComplexId Id { get; set; } = new () { Id = Guid.Empty, Version = 0 };

    object IEntity.Id => Id;

    public string? Name { get; set; }

    public string? ChangeToken { get; set; }

    public string? ExpectedChangeToken { get; set; }
}