// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;

namespace AppCoreNet.Data.EntityFramework.DAO; // Adjusted namespace

public class TestEntity2
{
    public Guid Id { get; set; }

    public int Version { get; set; }

    public string? Name { get; set; }

    public string? ChangeToken { get; set; }
}
