namespace AppCore.Data.EntityFrameworkCore;

public class EntityWithComplexId : IEntity<VersionId>
{
    public VersionId Id { get; set; }

    public string? Value { get; set; }

    object IEntity.Id => Id;
}