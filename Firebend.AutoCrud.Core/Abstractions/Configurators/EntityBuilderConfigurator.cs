using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Abstractions.Configurators
{
    public abstract class EntityBuilderConfigurator<TBuilder, TKey, TEntity>
        where TBuilder : EntityBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        public TBuilder Builder { get; }

        public EntityBuilderConfigurator(TBuilder builder)
        {
            Builder = builder;
        }
    }
}
