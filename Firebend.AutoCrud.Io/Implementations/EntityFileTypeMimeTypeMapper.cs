using System;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Io.Interfaces;
using Firebend.AutoCrud.Io.Models;

namespace Firebend.AutoCrud.Io.Implementations
{
    public class EntityFileTypeMimeTypeMapper<TVersion> : IEntityFileTypeMimeTypeMapper<TVersion>
        where TVersion : class, IAutoCrudApiVersion
    {
        public string MapMimeType(EntityFileType entityFileType) => entityFileType switch
        {
            EntityFileType.Csv => "text/csv",
            EntityFileType.Spreadsheet => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            EntityFileType.Unknown => throw new ArgumentException($"{nameof(EntityFileType.Unknown)} is not a valid entity file type"),
            _ => throw new ArgumentException($"Could not map mime type. Entity File Type: {entityFileType}")
        };

        public string GetExtension(EntityFileType entityFileType) => entityFileType switch
        {
            EntityFileType.Csv => ".csv",
            EntityFileType.Spreadsheet => ".xlsx",
            EntityFileType.Unknown => throw new ArgumentException($"{nameof(EntityFileType.Unknown)} is not a valid entity file type"),
            _ => throw new ArgumentException($"Could not map extension. Entity File Type: {entityFileType}")
        };
    }
}
