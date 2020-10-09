using System;
using Firebend.AutoCrud.Core.Abstractions;
using Firebend.AutoCrud.Mongo.Abstractions.Entities;

namespace Firebend.AutoCrud.Mongo
{
    public class MongoDbEntityBuilder : EntityCrudBuilder
    {
        public override Type CreateType { get; } = typeof(MongoEntityCreateService<,>);
        
        public override Type ReadType { get; } = typeof(MongoEntityReadService<,>);
        
        public override Type SearchType { get; } = typeof(MongoEntitySearchService<,,>);
        
        public override Type UpdateType { get; } = typeof(MongoEntityUpdateService<,>);
        
        public override Type DeleteType { get; } = typeof(MongoEntityDeleteService<,>);

        public override Type SoftDeleteType { get; } = typeof(MongoEntitySoftDeleteService<,>);
    }
}