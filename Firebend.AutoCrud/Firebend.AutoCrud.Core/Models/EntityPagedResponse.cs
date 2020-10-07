using System.Collections.Generic;
using Firebend.AutoCrud.Core.Interfaces;

namespace Firebend.AutoCrud.Core.Models
{
    public class EntityPagedResponse<TEntity, TKey>
        where TEntity : IEntity<TKey>
        where TKey : struct
    {
        public IEnumerable<TEntity> Data { get; set; }
        
        public int TotalRecords { get; set; }
        
        public int CurrentPage { get; set; }
        
        public int CurrentPageSize { get; set; }
    }
}