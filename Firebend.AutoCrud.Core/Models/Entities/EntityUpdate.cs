using System;
using System.Linq.Expressions;

namespace Firebend.AutoCrud.Core.Models.Entities
{
    public class EntityUpdate<T>
    {
        public T Entity { get; set; }

        public Expression<Func<T, bool>> Filter { get; set; }
    }
}
