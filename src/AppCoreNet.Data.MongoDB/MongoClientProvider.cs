// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using MongoDB.Driver;

namespace AppCoreNet.Data.MongoDB;

internal sealed class MongoClientProvider : IDisposable
{
    private readonly MongoClientSettings _settings;
    private IMongoClient? _client;
    private bool _disposed;

    public MongoClientProvider(MongoClientSettings settings)
    {
        _settings = settings;
    }

    public IMongoClient GetClient()
    {
        ThrowIfDisposed();

        lock (this)
        {
            return _client ??= new MongoClient(_settings);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _client?.Dispose();
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }
}