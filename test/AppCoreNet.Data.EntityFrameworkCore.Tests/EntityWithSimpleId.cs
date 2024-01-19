// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

namespace AppCoreNet.Data.EntityFrameworkCore;

public class EntityWithSimpleId : IEntity<int>
{
    public int Id { get; set; }

    public string? Value { get; set; }

    object IEntity.Id => Id;
}