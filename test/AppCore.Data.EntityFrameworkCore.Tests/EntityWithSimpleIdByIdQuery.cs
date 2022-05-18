namespace AppCore.Data.EntityFrameworkCore
{
    public class EntityWithSimpleIdByIdQuery : IQuery<EntityWithSimpleId, EntityWithSimpleId>
    {
        public int Id { get; set; }
    }
}