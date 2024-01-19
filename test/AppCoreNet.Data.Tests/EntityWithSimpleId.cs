// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

namespace AppCoreNet.Data;

public class EntityWithSimpleId : IEntity<int>
{
    public int Id { get; set; }

    object IEntity.Id => Id;
}