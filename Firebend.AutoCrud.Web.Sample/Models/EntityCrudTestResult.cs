namespace Firebend.AutoCrud.Web.Sample.Models;

public record EntityCrudTestResult<T>
{
    public bool PutWasCommitted { get; set; }
    public bool PatchWasCommitted { get; set; }
    public bool DeleteWasCommitted { get; set; }
    public bool CreateWasCommitted { get; set; }
    public bool ChangesCanBeReadInTransaction { get; set; }
    public T Read { get; set; }
    public T Created { get; set; }
}
