namespace AppCore.Data;

public class EntityWithSimpleId : IEntity<int>
{
    public int Id { get; set; }

    object IEntity.Id => Id;
}