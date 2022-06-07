using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Firebend.AutoCrud.Core.Exceptions;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations.Abstractions;

[SuppressMessage("ReSharper", "EF1001")]
public abstract class AbstractConstraintUpdateExceptionHandler<TKey, TEntity>
    : IEntityFrameworkDbUpdateExceptionHandler<TKey, TEntity>
    where TKey : struct
    where TEntity : IEntity<TKey>
{
    private const int SqlServerForeignKeyConstraintErrorCode = 547;
    private const int SqlServerUniqueConstraintErrorCode = 2601;

    public bool HandleException(IDbContext context, TEntity entity, DbUpdateException exception)
    {
        if (exception?.InnerException is not SqlException sqlException)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(sqlException.Message))
        {
            return false;
        }

        var hasHandledForeignKeyConstraint = sqlException.Number == SqlServerForeignKeyConstraintErrorCode
                                             && HandleForeignKeyConstraint(context, entity, sqlException);
        var hasHandledUniqueConstraint = sqlException.Number == SqlServerUniqueConstraintErrorCode
                                         && HandleUniqueConstraint(context, entity, sqlException);

        return hasHandledForeignKeyConstraint || hasHandledUniqueConstraint;
    }

    private static bool HandleUniqueConstraint(IDbContext context, TEntity entity, Exception sqlException)
    {
        if (context is not DbContext dbContext)
        {
            return false;
        }

        var constraintSplit = sqlException.Message.Split("index '");

        if (constraintSplit.Length < 2)
        {
            return false;
        }

        var constraint = constraintSplit[1][..constraintSplit[1].IndexOf("'", StringComparison.Ordinal)];

        var tableIndex = GetConstraint<TableIndex>(dbContext, constraint, x => x.Name);

        if (tableIndex == null)
        {
            return false;
        }

        var errors = tableIndex
            .MappedIndexes
            .SelectMany(x => x.Properties)
            .Select(x => (x.Name,
                $"{entity.GetType().GetProperty(x.Name)?.GetValue(entity, null)} already exists."))
            .ToArray();

        throw new AutoCrudEntityException("An entity has failed a db constraint",
            sqlException,
            entity,
            errors
        );

    }

    private static bool HandleForeignKeyConstraint(IDbContext context, TEntity entity, Exception sqlException)
    {
        if (context is not DbContext dbContext)
        {
            return false;
        }

        var constraintSplit = sqlException.Message.Split("constraint \"");

        if (constraintSplit.Length < 2)
        {
            return false;
        }

        var constraint = constraintSplit[1][..constraintSplit[1].IndexOf("\"", StringComparison.Ordinal)];

        var foreignKey = GetConstraint<ForeignKeyConstraint>(dbContext, constraint, x => x.Name);

        if (foreignKey == null)
        {
            return false;
        }

        var errors = foreignKey
            .MappedForeignKeys
            .SelectMany(x => x.Properties)
            .Select(x => (x.Name,
                $"Is part of a foreign key to {foreignKey.PrincipalTable.Name} that does not currently exist. Please check your reference."))
            .ToArray();

        throw new AutoCrudEntityException("An entity has failed a db constraint",
            sqlException,
            entity,
            errors
        );
    }

    private static T GetConstraint<T>(DbContext dbContext,
        string constraintName,
        Func<T, string> nameSelector) where T : IAnnotatable
    {
        var modelType = dbContext.Model.FindEntityType(typeof(TEntity));

        if (modelType is null)
        {
            return default;
        }

        var constraint = modelType
            .GetIndexes()
            .SelectMany(x => x.GetAnnotations().Concat(x.GetRuntimeAnnotations()))
            .Where(x => x?.Value != null)
            .Where(x => x.Value is IEnumerable<T>)
            .SelectMany(x => x.Value as IEnumerable<T>)
            .Where(x => x is not null)
            .FirstOrDefault(x => nameSelector(x).EqualsIgnoreCaseAndWhitespace(constraintName));

        return constraint;
    }
}
