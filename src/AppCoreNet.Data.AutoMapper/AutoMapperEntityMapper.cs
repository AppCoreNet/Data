// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using AppCoreNet.Diagnostics;
using AutoMapper;

namespace AppCoreNet.Data.AutoMapper;

/// <summary>
/// Provides an implementation of <see cref="IEntityMapper"/> which uses AutoMapper.
/// </summary>
public sealed class AutoMapperEntityMapper : IEntityMapper
{
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoMapperEntityMapper"/> class.
    /// </summary>
    /// <param name="mapper">The <see cref="IMapper"/>.</param>
    public AutoMapperEntityMapper(IMapper mapper)
    {
        Ensure.Arg.NotNull(mapper);
        _mapper = mapper;
    }

    /// <inheritdoc />
    public void Map<TFrom, TTo>(TFrom from, TTo to)
    {
        _mapper.Map(from, to);
    }

    /// <inheritdoc />
    public TTo Map<TTo>(object from)
    {
        return _mapper.Map<TTo>(from);
    }
}