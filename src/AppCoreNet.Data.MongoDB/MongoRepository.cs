using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using AppCoreNet.Diagnostics;
using Microsoft.Extensions.Primitives;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace AppCoreNet.Data.MongoDB;

public class MongoRepository<TId, TEntity, TDocument> : IRepository<TId, TEntity>, IMongoRepository
    where TEntity : class, IEntity<TId>
    where TDocument : class
{
    private const string ObjectIdField = "_id";

    [SuppressMessage(
        "ReSharper",
        "StaticMemberInGenericType",
        Justification = "Depends on generic parameter.")]
    private static readonly string? _changeTokenField;

    public MongoDataProvider Provider { get; }

    protected IMongoCollection<BsonDocument> Collection { get; }

    IDataProvider IRepository<TId, TEntity>.Provider => Provider;

    static MongoRepository()
    {
        if (typeof(IHasChangeToken).IsAssignableFrom(typeof(TEntity))
            || typeof(IHasChangeTokenEx).IsAssignableFrom(typeof(TEntity)))
        {
            IBsonSerializer<TDocument> serializer = BsonSerializer.LookupSerializer<TDocument>();
            var documentSerializer = (IBsonDocumentSerializer)serializer;

            if (documentSerializer.TryGetMemberSerializationInfo(
                    nameof(IHasChangeToken.ChangeToken),
                    out BsonSerializationInfo memberInfo))
            {
                _changeTokenField = memberInfo.ElementName;
            }
            else
            {
                throw new InvalidOperationException(
                    $"Entity implements change token but no '{nameof(IHasChangeToken.ChangeToken)}' property has been found in '{typeof(TDocument)}'");
            }
        }
    }

    public MongoRepository(MongoDataProvider provider, string? collectionName = null)
    {
        Ensure.Arg.NotNull(provider);

        Provider = provider;
        Collection = provider.Database.GetCollection<BsonDocument>(
            string.IsNullOrEmpty(collectionName) ? provider.GetCollectionName<TEntity>() : collectionName,
            new MongoCollectionSettings
            {
                AssignIdOnInsert = true,
            });
    }

    private FilterDefinition<BsonDocument> GetModificationFilter(TEntity entity)
    {
        FilterDefinitionBuilder<BsonDocument> builder = Builders<BsonDocument>.Filter;
        FilterDefinition<BsonDocument> filter = builder.Eq(ObjectIdField, entity.Id);

        switch (entity)
        {
            case IHasChangeToken changeToken:
                filter &= builder.Eq(_changeTokenField, changeToken.ChangeToken);
                break;
            case IHasChangeTokenEx changeTokenEx:
                filter &= builder.Eq(_changeTokenField, changeTokenEx.ExpectedChangeToken);
                break;
        }

        return filter;
    }

    private void UpdateChangeToken(TEntity entity, BsonDocument document)
    {
        switch (entity)
        {
            case IHasChangeToken:
                document.Set(_changeTokenField, new BsonString(Provider.TokenGenerator.Generate()));
                break;

            case IHasChangeTokenEx changeTokenEx:
                string? changeToken = changeTokenEx.ChangeToken;
                if (string.IsNullOrWhiteSpace(changeToken)
                    || string.Equals(changeToken, changeTokenEx.ExpectedChangeToken))
                {
                    document.Set(_changeTokenField, new BsonString(Provider.TokenGenerator.Generate()));
                }

                break;
        }
    }

    public async Task<TResult> QueryAsync<TResult>(IQuery<TEntity, TResult> query, CancellationToken cancellationToken = default)
    {
        Ensure.Arg.NotNull(query);

        IMongoQueryHandler<TEntity, TResult> queryHandler =
            Provider.QueryHandlerFactory.CreateHandler(Provider, query);

        Type queryType = query.GetType();
        // _logger.QueryExecuting(queryType);

        var stopwatch = new Stopwatch();

        TResult result;
        try
        {
            result = await queryHandler.ExecuteAsync(query, cancellationToken)
                                       .ConfigureAwait(false);

            // _logger.QueryExecuted(queryType, stopwatch.Elapsed);
        }
        catch (Exception error)
        {
            // TODO: _logger.QueryFailed(error, queryType);
            throw;
        }
        finally
        {
            switch (queryHandler)
            {
                case IAsyncDisposable disposable:
                    await disposable.DisposeAsync()
                                    .ConfigureAwait(false);
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }

        return result;
    }

    protected virtual async Task<TDocument?> FindCoreAsync(TId id, CancellationToken cancellationToken)
    {
        IClientSessionHandle? sessionHandle = Provider.TransactionManager.CurrentTransaction?.SessionHandle;

        FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Eq("_id", id);

        IFindFluent<BsonDocument, BsonDocument> find = sessionHandle != null
            ? Collection.Find(sessionHandle, filter)
            : Collection.Find(filter);

        return await find.As<TDocument>()
                         .FirstOrDefaultAsync(cancellationToken)
                         .ConfigureAwait(false);
    }

    public async Task<TEntity?> FindAsync(TId id, CancellationToken cancellationToken = default)
    {
        Ensure.Arg.NotNull(id);

        TDocument? document = await FindCoreAsync(id, cancellationToken)
            .ConfigureAwait(false);

        return document != null
            ? Provider.EntityMapper.Map<TEntity>(document)
            : default;
    }

    public async Task<TEntity> LoadAsync(TId id, CancellationToken cancellationToken = default)
    {
        TEntity? entity = await FindAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(TEntity), id!);
        }

        return entity;
    }

    protected virtual async Task<TDocument> UpdateCoreAsync(
        TEntity entity,
        TDocument document,
        CancellationToken cancellationToken)
    {
        IClientSessionHandle? sessionHandle = Provider.TransactionManager.CurrentTransaction?.SessionHandle;

        FilterDefinition<BsonDocument> filter = GetModificationFilter(entity);

        var bson = document.ToBsonDocument();
        UpdateChangeToken(entity, bson);

        ReplaceOneResult result;
        if (sessionHandle == null)
        {
            result = await Collection.ReplaceOneAsync(
                                         filter,
                                         bson,
                                         cancellationToken: cancellationToken)
                                     .ConfigureAwait(false);
        }
        else
        {
            result = await Collection.ReplaceOneAsync(
                                         sessionHandle,
                                         filter,
                                         bson,
                                         cancellationToken: cancellationToken)
                                     .ConfigureAwait(false);
        }

        if (result.ModifiedCount == 0)
        {
            throw new EntityConcurrencyException();
        }

        return BsonSerializer.Deserialize<TDocument>(bson);
    }

    public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Ensure.Arg.NotNull(entity);

        if (entity.IsTransient())
        {
            throw new InvalidOperationException(
                $"The entity cannot be updated because the '{nameof(IEntity<TId>.Id)}' property has the default value.");
        }

        var document = Provider.EntityMapper.Map<TDocument>(entity);

        document = await UpdateCoreAsync(entity, document, cancellationToken)
            .ConfigureAwait(false);

        return Provider.EntityMapper.Map<TEntity>(document);
    }

    protected virtual async Task<TDocument> CreateCoreAsync(
        TEntity entity,
        TDocument document,
        CancellationToken cancellationToken)
    {
        IClientSessionHandle? sessionHandle = Provider.TransactionManager.CurrentTransaction?.SessionHandle;

        var bson = document.ToBsonDocument();
        UpdateChangeToken(entity, bson);

        if (sessionHandle == null)
        {
            await Collection.InsertOneAsync(bson, cancellationToken: cancellationToken)
                            .ConfigureAwait(false);
        }
        else
        {
            await Collection.InsertOneAsync(sessionHandle, bson, cancellationToken: cancellationToken)
                            .ConfigureAwait(false);
        }

        return BsonSerializer.Deserialize<TDocument>(bson);
    }

    public async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Ensure.Arg.NotNull(entity);

        var document = Provider.EntityMapper.Map<TDocument>(entity);

        document = await CreateCoreAsync(entity, document, cancellationToken)
            .ConfigureAwait(false);

        return Provider.EntityMapper.Map<TEntity>(document);
    }

    protected virtual async Task DeleteCoreAsync(TEntity entity, CancellationToken cancellationToken)
    {
        IClientSessionHandle? sessionHandle = Provider.TransactionManager.CurrentTransaction?.SessionHandle;
        FilterDefinition<BsonDocument> filter = GetModificationFilter(entity);

        DeleteResult result;
        if (sessionHandle == null)
        {
            result = await Collection.DeleteOneAsync(filter, cancellationToken: cancellationToken)
                                     .ConfigureAwait(false);
        }
        else
        {
            result = await Collection.DeleteOneAsync(sessionHandle, filter, cancellationToken: cancellationToken)
                                     .ConfigureAwait(false);
        }

        if (result.DeletedCount == 0)
        {
            throw new EntityConcurrencyException();
        }
    }

    public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Ensure.Arg.NotNull(entity);

        await DeleteCoreAsync(entity, cancellationToken)
            .ConfigureAwait(false);
    }
}