using System.ComponentModel.DataAnnotations;

namespace AppCore.Data.EntityFrameworkCore;

public class DbEntityWithSimpleId
{
    public int Id { get; set; }

    public string? Value { get; set; }
}