// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

namespace AppCoreNet.Data;

/// <summary>
/// Represents a generator for arbitrary tokens.
/// </summary>
public interface ITokenGenerator
{
    /// <summary>
    /// Generates a new token.
    /// </summary>
    /// <returns>The token.</returns>
    string Generate();
}