namespace Firebend.AutoCrud.Core.Models.Entities
{
    public class TenantEntityResult<TKey> where TKey : struct
    {
        public TKey TenantId { get; set; }
    }
}
