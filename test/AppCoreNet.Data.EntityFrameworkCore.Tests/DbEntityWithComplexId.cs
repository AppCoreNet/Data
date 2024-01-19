namespace AppCoreNet.Data.EntityFrameworkCore;

public class DbEntityWithComplexId
{
    public int Id { get; set; }

    public int Version { get; set; }

    public string? Value { get; set; }
}