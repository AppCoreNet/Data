namespace AppCoreNet.Data.MongoDB;

public interface IMongoRepository
{
    MongoDataProvider Provider { get; }
}