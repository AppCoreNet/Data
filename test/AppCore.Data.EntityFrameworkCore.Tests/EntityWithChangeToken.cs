namespace AppCore.Data.EntityFrameworkCore
{
    public class EntityWithChangeToken : IEntity<int>, IHasChangeToken
    {
        object IEntity.Id => Id;

        public int Id { get; set; }

        public string? ChangeToken { get; set; }
    }
}