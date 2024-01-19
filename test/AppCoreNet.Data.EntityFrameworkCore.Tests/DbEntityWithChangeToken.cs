namespace AppCoreNet.Data.EntityFrameworkCore;

public class DbEntityWithChangeToken
{
    public int Id { get; set; }

    public string? ChangeToken { get; set; }
}