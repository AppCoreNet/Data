// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;

namespace AppCoreNet.Data;

/// <summary>
/// Provides arbitrary token generator based on https://hashids.org/.
/// </summary>
public class TokenGenerator : ITokenGenerator
{
    private readonly Hashids _hashIds = new Hashids();

    /// <inheritdoc />
    public string Generate()
    {
        return _hashIds.EncodeHex(Guid.NewGuid().ToString("N"));
    }
}