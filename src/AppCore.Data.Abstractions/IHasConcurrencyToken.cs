// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

namespace AppCore.Data
{
    /// <summary>
    /// Represents an entity with a concurrency token.
    /// </summary>
    public interface IHasConcurrencyToken
    {
        /// <summary>
        /// Gets the concurrency token.
        /// </summary>
        string ConcurrencyToken { get; }
    }
}