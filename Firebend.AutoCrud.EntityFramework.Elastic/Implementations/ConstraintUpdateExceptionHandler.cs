using System;
using System.Collections.Generic;
using System.Linq;
using Firebend.AutoCrud.Core.Exceptions;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations
{
    public class ConstraintUpdateExceptionHandler<TKey, TEntity>
        : IEntityFrameworkDbUpdateExceptionHandler<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        public bool HandleException(IDbContext context, TEntity entity, DbUpdateException exception)
        {
            if (!(exception?.InnerException is SqlException sqlException))
            {
                return false;
            }

            return sqlException.Number == 547 && HandleConstraint(context, entity, sqlException);
        }

        private static bool HandleConstraint(IDbContext context, TEntity entity, Exception sqlException)
        {
            var constraintSplit = sqlException.Message.Split("constraint \"");
            var constraint = constraintSplit[1].Substring(0, constraintSplit[1].IndexOf("\""));

            if (!(context is DbContext dbContext))
            {
                return false;
            }

            var foreignKey = dbContext
                .Model
                .FindEntityType(typeof(TEntity))
                .GetForeignKeys()
                .SelectMany(x => x.GetAnnotations())
                .Where(x => x.Value is IEnumerable<ForeignKeyConstraint>)
                .SelectMany(x => x.Value as IEnumerable<ForeignKeyConstraint>)
                .FirstOrDefault(x => x?.Name?.EqualsIgnoreCaseAndWhitespace(constraint) ?? false);

            if (foreignKey == null)
            {
                return false;
            }

            {
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
}