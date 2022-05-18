namespace AppCore.Data.EntityFrameworkCore
{
    public class EntityWithChangeTokenEx : IEntity<int>, IHasChangeTokenEx
    {
        object IEntity.Id => Id;

        public int Id { get; set; }

        public string? ChangeToken { get; set; }

        public string? ExpectedChangeToken { get; set; }
    }
}