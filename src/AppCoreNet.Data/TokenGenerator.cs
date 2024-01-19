// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using System;

namespace AppCore.Data;

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