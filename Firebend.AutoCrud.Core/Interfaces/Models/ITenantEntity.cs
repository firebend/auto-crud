namespace Firebend.AutoCrud.Core.Interfaces.Models;

public interface ITenantEntity<TKey>
    where TKey : struct
{
    public TKey TenantId { get; set; }
}
