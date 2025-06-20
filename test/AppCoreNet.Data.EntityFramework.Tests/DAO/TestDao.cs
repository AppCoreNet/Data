// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;

namespace AppCoreNet.Data.EntityFramework.DAO;

public class TestDao
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? ChangeToken { get; set; }
}
