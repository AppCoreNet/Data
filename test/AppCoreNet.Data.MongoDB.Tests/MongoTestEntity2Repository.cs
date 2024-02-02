namespace AppCoreNet.Data.MongoDB;

public class MongoTestEntity2Repository
    : MongoRepository<Entities.ComplexId, Entities.TestEntity2, DAO.TestEntity2>, ITestEntity2Repository
{
    public MongoTestEntity2Repository(MongoDataProvider provider)
        : base(provider)
    {
    }
}