using System;

namespace AppCoreNet.Data.MongoDB.DAO;

public class ComplexId
{
    public Guid Id { get; set; }

    public int Version { get; set; }
}