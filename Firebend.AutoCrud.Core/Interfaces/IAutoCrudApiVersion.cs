namespace Firebend.AutoCrud.Core.Interfaces;

public interface IAutoCrudApiVersion
{
    public int Version { get; }
    public int MinorVersion => 0;
    public string Name { get; }
}
