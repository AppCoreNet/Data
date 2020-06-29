namespace AppCore.Data
{
    public class EntityWithSimpleId : IEntity<int>
    {
        public int Id { get; set; }
    }
}