// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

namespace AppCoreNet.Data.EntityFrameworkCore;

public class EntityWithChangeToken : IEntity<int>, IHasChangeToken
{
    object IEntity.Id => Id;

    public int Id { get; set; }

    public string? ChangeToken { get; set; }
}