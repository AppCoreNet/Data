// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

namespace AppCoreNet.Data.EntityFrameworkCore;

public class DbEntityWithComplexId
{
    public int Id { get; set; }

    public int Version { get; set; }

    public string? Value { get; set; }
}