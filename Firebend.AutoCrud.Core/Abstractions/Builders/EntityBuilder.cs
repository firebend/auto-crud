using System;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Abstractions.Builders
{
    public abstract class EntityBuilder<TKey, TEntity> : BaseBuilder
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        public string EntityName { get; set; }

        public Type EntityType => typeof(TEntity);

        public Type EntityKeyType => typeof(TKey);

        public Type ExportType { get; set; }

        public override string SignatureBase => $"{EntityType.Name}_{EntityName}";
    }
}
