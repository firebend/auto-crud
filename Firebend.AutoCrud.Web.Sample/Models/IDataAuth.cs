namespace Firebend.AutoCrud.Web.Sample.Models;

public interface IDataAuth
{
    string[] UserEmails { get; set; }
}

public class DataAuth : IDataAuth
{
    public string[] UserEmails { get; set; } = System.Array.Empty<string>();
}

public interface IEntityDataAuth
{
    DataAuth DataAuth { get; set; }
}
