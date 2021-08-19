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
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations.Abstractions
{
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
            if (!(exception?.InnerException is SqlException sqlException))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(sqlException?.Message))
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
            if (!(context is DbContext dbContext))
            {
                return false;
            }

            var constraintSplit = sqlException.Message.Split("index '");
            var constraint = constraintSplit[1].Substring(0, constraintSplit[1].IndexOf("'", StringComparison.Ordinal));

            var tableIndex = dbContext
                .Model
                .FindEntityType(typeof(TEntity))
                .GetIndexes()
                .SelectMany(x => x.GetAnnotations())
                .Where(x => x?.Value != null)
                .Where(x => x.Value is IEnumerable<TableIndex>)
                .SelectMany(x => x.Value as IEnumerable<TableIndex>)
                .FirstOrDefault(x => x?.Name?.EqualsIgnoreCaseAndWhitespace(constraint) ?? false);

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
            if (!(context is DbContext dbContext))
            {
                return false;
            }

            var constraintSplit = sqlException.Message.Split("constraint \"");
            var constraint = constraintSplit[1].Substring(0, constraintSplit[1].IndexOf("\"", StringComparison.Ordinal));

            var foreignKey = dbContext
                .Model
                .FindEntityType(typeof(TEntity))
                .GetForeignKeys()
                .SelectMany(x => x.GetAnnotations())
                .Where(x => x?.Value != null)
                .Where(x => x.Value is IEnumerable<ForeignKeyConstraint>)
                .SelectMany(x => x.Value as IEnumerable<ForeignKeyConstraint>)
                .FirstOrDefault(x => x?.Name?.EqualsIgnoreCaseAndWhitespace(constraint) ?? false);

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
    }
}
