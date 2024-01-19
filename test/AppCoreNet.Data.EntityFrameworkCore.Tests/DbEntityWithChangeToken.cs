// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

namespace AppCoreNet.Data.EntityFrameworkCore;

public class DbEntityWithChangeToken
{
    public int Id { get; set; }

    public string? ChangeToken { get; set; }
}