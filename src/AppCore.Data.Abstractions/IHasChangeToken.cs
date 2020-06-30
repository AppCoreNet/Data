// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

namespace AppCore.Data
{
    /// <summary>
    /// Represents an entity with a concurrency token.
    /// </summary>
    public interface IHasChangeToken
    {
        /// <summary>
        /// Gets the concurrency token.
        /// </summary>
        string ChangeToken { get; }
    }
}