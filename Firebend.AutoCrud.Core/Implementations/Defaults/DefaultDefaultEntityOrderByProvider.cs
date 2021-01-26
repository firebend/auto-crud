using System;
using System.Linq.Expressions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;

namespace Firebend.AutoCrud.Core.Implementations.Defaults
{
    public class DefaultDefaultEntityOrderByProvider<TKey, TEntity> : IDefaultEntityOrderByProvider<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        private readonly Expression<Func<TEntity, object>> _expression;
        private readonly bool _ascending;

        public DefaultDefaultEntityOrderByProvider()
        {

        }
        public DefaultDefaultEntityOrderByProvider(Expression<Func<TEntity, object>> expression, bool ascending)
        {
            _expression = expression;
            _ascending = ascending;
        }

        public (Expression<Func<TEntity, object>> order, bool ascending) GetOrderBy() => (_expression, _ascending);
    }

    public class DefaultEntityOrderByProviderModified<TKey, TEntity> : DefaultDefaultEntityOrderByProvider<TKey, TEntity>
        where TEntity : IEntity<TKey>, IModifiedEntity
        where TKey : struct
    {
        public DefaultEntityOrderByProviderModified() : base(x => x.ModifiedDate, false)
        {
        }
    }

    public class DefaultEntityOrderByProviderActive<TKey, TEntity> : DefaultDefaultEntityOrderByProvider<TKey, TEntity>
        where TEntity : IEntity<TKey>, IActiveEntity
        where TKey : struct
    {
        public DefaultEntityOrderByProviderActive() : base(x => x.IsDeleted, false)
        {
        }
    }
}
