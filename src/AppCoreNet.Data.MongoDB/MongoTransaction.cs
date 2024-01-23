using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace AppCoreNet.Data.MongoDB;

public sealed class MongoTransaction : ITransaction
{
    private readonly IClientSessionHandle _session;

    public IClientSessionHandle SessionHandle => _session;

    public event EventHandler? TransactionFinished;

    private MongoTransaction(IClientSessionHandle session)
    {
        _session = session;
    }

    public static MongoTransaction Create(IMongoClient client, ClientSessionOptions options)
    {
        IClientSessionHandle session = client.StartSession(options);
        session.StartTransaction();
        return new MongoTransaction(session);
    }

    public static async Task<MongoTransaction> CreateAsync(
        IMongoClient client,
        ClientSessionOptions options,
        CancellationToken cancellationToken = default)
    {
        IClientSessionHandle session = await client.StartSessionAsync(options, cancellationToken)
                                                   .ConfigureAwait(false);

        session.StartTransaction();
        return new MongoTransaction(session);
    }

    public void Dispose()
    {
        if (_session.IsInTransaction)
        {
            Rollback();
        }

        _session.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_session.IsInTransaction)
        {
            await RollbackAsync()
                .ConfigureAwait(false);
        }

        _session.Dispose();
    }

    public void Commit()
    {
        _session.CommitTransaction();
        TransactionFinished?.Invoke(this, EventArgs.Empty);
    }

    public void Rollback()
    {
        _session.AbortTransaction();
        TransactionFinished?.Invoke(this, EventArgs.Empty);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await _session.CommitTransactionAsync(cancellationToken)
                      .ConfigureAwait(false);

        TransactionFinished?.Invoke(this, EventArgs.Empty);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await _session.AbortTransactionAsync(cancellationToken)
                      .ConfigureAwait(false);

        TransactionFinished?.Invoke(this, EventArgs.Empty);
    }
}