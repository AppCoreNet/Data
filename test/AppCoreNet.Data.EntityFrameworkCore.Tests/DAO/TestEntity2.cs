using System;

namespace AppCoreNet.Data.EntityFrameworkCore.DAO;

public class TestEntity2
{
    public Guid Id { get; set; }

    public int Version { get; set; }

    public string? Name { get; set; }

    public string? ChangeToken { get; set; }
}