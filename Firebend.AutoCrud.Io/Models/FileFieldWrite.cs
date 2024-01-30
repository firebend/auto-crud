using System;

namespace Firebend.AutoCrud.Io.Models;

public class FileFieldWrite<T> : FileField, IFileFieldWrite<T>
{
    public Func<T, object> Writer { get; set; }
}
