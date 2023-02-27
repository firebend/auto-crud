namespace Firebend.AutoCrud.Core.Interfaces;

public interface IApiVersion
{
    public int Version { get; }
    public int MinorVersion => 0;
    public string Name { get; }
}
