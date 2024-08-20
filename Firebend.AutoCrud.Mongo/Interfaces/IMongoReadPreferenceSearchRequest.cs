namespace Firebend.AutoCrud.Mongo.Interfaces;

public interface IMongoReadPreferenceSearchRequest
{
    public bool IsReadFromSecondary { get; set; }
}
