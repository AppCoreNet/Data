// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

namespace AppCoreNet.Data.EntityFrameworkCore;

public class EntityWithSimpleIdByIdQuery : IQuery<EntityWithSimpleId, EntityWithSimpleId?>
{
    public int Id { get; set; }
}