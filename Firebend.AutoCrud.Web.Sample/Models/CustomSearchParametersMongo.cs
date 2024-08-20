using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.Web.Sample.Models;

public class CustomSearchParametersMongo : CustomSearchParameters, IMongoReadPreferenceSearchRequest
{
    public bool IsReadFromSecondary { get; set; }
}
