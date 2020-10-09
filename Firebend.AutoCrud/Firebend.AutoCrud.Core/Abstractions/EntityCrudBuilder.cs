using System;

namespace Firebend.AutoCrud.Core.Abstractions
{
    public abstract class EntityCrudBuilder : EntityBuilder
    {
        public abstract Type CreateType { get; }
        
        public abstract Type ReadType { get; }
        
        public abstract Type SearchType { get; }
        
        public abstract Type UpdateType { get; set; }
        
        public abstract Type DeleteType { get; }
    }
}