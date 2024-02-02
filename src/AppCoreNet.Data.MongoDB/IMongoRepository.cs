// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using AppCoreNet.Extensions.DependencyInjection;

namespace AppCoreNet.Data.MongoDB;

/// <summary>
/// Represent a MongoDB repository.
/// </summary>
/// <remarks>
/// Primarily used as a marker interface by <see cref="MongoDataProviderBuilderExtensions"/>.
/// </remarks>
public interface IMongoRepository
{
    /// <summary>
    /// Gets the <see cref="MongoDataProvider"/> of the repository.
    /// </summary>
    MongoDataProvider Provider { get; }
}