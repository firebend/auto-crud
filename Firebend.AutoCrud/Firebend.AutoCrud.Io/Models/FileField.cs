using System;

namespace Firebend.AutoCrud.Io.Models
{
    public interface IFileField
    {
        int FieldIndex { get; }

        string FieldName { get; }
    }

    public class FileField : IFileField
    {
        public int FieldIndex { get; set; }

        public string FieldName { get; set; }
    }

    public interface IFileFieldWrite<T> : IFileField
    {
        Func<T, object> Writer { get; }
    }

    public class FileFieldWrite<T> : FileField, IFileFieldWrite<T>
    {
        public Func<T, object> Writer { get; set; }
    }
}
