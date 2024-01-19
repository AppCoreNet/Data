// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

namespace AppCoreNet.Data;

/// <summary>
/// Represents an entity which implements optimistic concurrency checks with a automatically managed change token.
/// </summary>
public interface IHasChangeToken
{
    /// <summary>
    /// Gets the current/expected concurrency token.
    /// </summary>
    /// <remarks>
    /// The property contains the current change token after the entity has been loaded.
    /// After the entity has been saved the change token will have a updated value.
    /// </remarks>
    string? ChangeToken { get; }
}