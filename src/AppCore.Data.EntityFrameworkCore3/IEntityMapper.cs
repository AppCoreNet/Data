// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

namespace AppCore.Data.EntityFrameworkCore
{
    public interface IEntityMapper
    {
        void Map<TFrom, TTo>(TFrom from, TTo to);

        TTo Map<TTo>(object from);
    }
}