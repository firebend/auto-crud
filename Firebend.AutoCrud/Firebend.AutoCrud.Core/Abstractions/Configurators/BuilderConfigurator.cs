using Firebend.AutoCrud.Core.Abstractions.Builders;

namespace Firebend.AutoCrud.Core.Abstractions.Configurators
{
    public abstract class BuilderConfigurator<TBuilder>
        where TBuilder : EntityBuilder
    {
        public TBuilder Builder { get;  }
        
        public BuilderConfigurator(TBuilder builder)
        {
            Builder = builder;
        }
    }
}