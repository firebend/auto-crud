namespace Firebend.AutoCrud.Core.Interfaces.Models;

public interface ITenantEntity<TKey>
    where TKey : struct
{
    TKey TenantId { get; set; }
}
