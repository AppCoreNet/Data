namespace AppCore.Data.EntityFrameworkCore;

public class EntityWithSimpleId : IEntity<int>
{
    public int Id { get; set; }

    public string? Value { get; set; }

    object IEntity.Id => Id;
}