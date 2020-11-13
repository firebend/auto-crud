using System;
using Firebend.AutoCrud.Io.Interfaces;
using Firebend.AutoCrud.Io.Models;

namespace Firebend.AutoCrud.Io.Implementations
{
    public class EntityFileTypeMimeTypeMapper : IEntityFileTypeMimeTypeMapper
    {
        public string MapMimeType(EntityFileType entityFileType) => entityFileType switch
        {
            EntityFileType.Csv => "text/csv",
            EntityFileType.Spreadsheet => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => throw new ArgumentException($"Could not map mime type. Entity File Type: {entityFileType}")
        };

        public string GetExtension(EntityFileType entityFileType) => entityFileType switch
        {
            EntityFileType.Csv => ".csv",
            EntityFileType.Spreadsheet => ".xlsx",
            _ => throw new ArgumentException($"Could not map extension. Entity File Type: {entityFileType}")
        };
    }
}
