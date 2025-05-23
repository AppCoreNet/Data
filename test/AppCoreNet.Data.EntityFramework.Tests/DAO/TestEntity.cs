// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.ComponentModel.DataAnnotations;

namespace AppCoreNet.Data.EntityFramework.DAO; // Adjusted namespace

public class TestEntity
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? ChangeToken { get; set; }
}
