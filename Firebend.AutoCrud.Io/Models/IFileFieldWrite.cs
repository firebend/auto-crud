using System;

namespace Firebend.AutoCrud.Io.Models;

public interface IFileFieldWrite<T> : IFileField
{
    Func<T, object> Writer { get; }
}
