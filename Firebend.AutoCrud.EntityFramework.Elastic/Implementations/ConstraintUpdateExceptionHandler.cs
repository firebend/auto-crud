using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Firebend.AutoCrud.Core.Exceptions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Firebend.JsonPatch.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations;

[SuppressMessage("ReSharper", "EF1001")]
public class ConstraintUpdateExceptionHandler<TKey, TEntity>
    : IEntityFrameworkDbUpdateExceptionHandler<TKey, TEntity>
    where TKey : struct
    where TEntity : IEntity<TKey>
{
    private const int SqlServerConstraintErrorCode = 547;
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

        var hasHandledForeignKeyConstraint = sqlException.Number == SqlServerConstraintErrorCode
                                             && HandleConstraint(context, entity, sqlException);

        var hasHandledUniqueConstraint = sqlException.Number == SqlServerUniqueConstraintErrorCode
                                         && HandleUniqueConstraint(context, entity, sqlException);

        return hasHandledForeignKeyConstraint || hasHandledUniqueConstraint;
    }


    protected virtual bool HandleUniqueConstraint(IDbContext context, TEntity entity, SqlException sqlException)
    {
        if (context is not DbContext dbContext)
        {
            return false;
        }

        var constraintSplit = sqlException.Message.Split("index '");

        if (constraintSplit.Length < 2)
        {
            return OnUniqueConstraintUnresolved(entity, sqlException);
        }

        var constraint = constraintSplit[1][..constraintSplit[1].IndexOf('\'')];

        var tableIndex = GetConstraint<TableIndex, IIndex>(
            dbContext,
            constraint,
            x => x.GetIndexes(),
            x => x.Name);

        if (tableIndex == null)
        {
            return OnUniqueConstraintUnresolved(entity, sqlException);
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

    /// <summary>
    /// Occurs when the handler is unable to figure out the Unique Constraint.
    /// Use for custom handling
    /// </summary>
    /// <param name="exception"></param>
    /// <param name="sqlException"></param>
    /// <returns>
    /// True if the error should be handled; otherwise, false
    /// </returns>
    protected virtual bool OnUniqueConstraintUnresolved(TEntity exception, SqlException sqlException) => false;

    protected virtual bool HandleConstraint(IDbContext context, TEntity entity, SqlException sqlException)
    {
        if (context is not DbContext dbContext)
        {
            return false;
        }

        var constraintSplit = sqlException.Message.Split("constraint \"");

        if (constraintSplit.Length < 2)
        {
            return OnConstraintUnresolved(entity, sqlException);
        }

        var constraint = constraintSplit[1][..constraintSplit[1].IndexOf('"')];

        var foreignKey = GetConstraint<ForeignKeyConstraint, IForeignKey>(
            dbContext,
            constraint,
            x => x.GetForeignKeys(),
            x => x.Name);

        if (foreignKey == null)
        {
            return OnConstraintUnresolved(entity, sqlException);
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

    /// <summary>
    /// Occurs when the handler is unable to figure out the Foreign Key.
    /// Use for custom handling
    /// </summary>
    /// <param name="exception"></param>
    /// <param name="sqlException"></param>
    /// <returns>
    /// True if the error should be handled; otherwise, false
    /// </returns>
    protected virtual bool OnConstraintUnresolved(TEntity exception, SqlException sqlException) => false;

    protected virtual TConstraint GetConstraint<TConstraint, TConstrainList>(DbContext dbContext,
        string constraintName,
        Func<IEntityType, IEnumerable<TConstrainList>> constraintListSelector,
        Func<TConstraint, string> nameSelector)
        where TConstraint : IAnnotatable
        where TConstrainList : IAnnotatable
    {
        var modelType = dbContext.Model.FindEntityType(typeof(TEntity));

        if (modelType is null)
        {
            return default;
        }

        var constraint = constraintListSelector(modelType)
            .SelectMany(x => x.GetAnnotations().Concat(x.GetRuntimeAnnotations()))
            .Where(x => x?.Value != null)
            .Where(x => x.Value is IEnumerable<TConstraint>)
            .SelectMany(x => x.Value as IEnumerable<TConstraint>)
            .Where(x => x is not null)
            .FirstOrDefault(x => nameSelector(x).EqualsIgnoreCaseAndWhitespace(constraintName));

        return constraint;
    }
}
