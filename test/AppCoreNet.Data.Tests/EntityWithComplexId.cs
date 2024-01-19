// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

namespace AppCoreNet.Data;

public class EntityWithComplexId : IEntity<VersionId>
{
    public VersionId Id { get; set; }

    object IEntity.Id => Id;
}