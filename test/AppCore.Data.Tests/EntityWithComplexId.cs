namespace AppCore.Data
{
    public class EntityWithComplexId : IEntity<VersionId>
    {
        public VersionId Id { get; set; }
    }
}