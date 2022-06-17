namespace Firebend.AutoCrud.Web.Sample.Models;

public class SessionTransactionAssertionViewModel
{
    public EntityCrudTestResult<GetPersonViewModel> Ef { get; set; }
    public EntityCrudTestResult<GetPersonViewModel> Mongo { get; set; }
    public string ExceptionMessage { get; set; }
}
