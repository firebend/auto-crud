using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.Io.Interfaces;
using Firebend.AutoCrud.Io.Models;
using Firebend.AutoCrud.Io.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Firebend.AutoCrud.Io.Web.Abstractions
{
    public abstract class AbstractEntityExportControllerService<TKey, TEntity, TSearch, TMapped> :
        IEntityExportControllerService<TSearch>
        where TSearch : EntitySearchRequest
        where TMapped : class
        where TEntity : class, IEntity<TKey>
        where TKey : struct
    {
        private readonly IEntityFileTypeMimeTypeMapper _entityFileTypeMimeTypeMapper;
        private readonly IEntityExportService<TMapped> _exportService;
        private readonly IEntityExportMapper<TEntity, TMapped> _mapper;
        private readonly IEntitySearchService<TKey,TEntity,TSearch> _searchService;

        public AbstractEntityExportControllerService(IEntityFileTypeMimeTypeMapper entityFileTypeMimeTypeMapper,
            IEntityExportService<TMapped> exportService,
            IEntityExportMapper<TEntity, TMapped> mapper,
            IEntitySearchService<TKey, TEntity, TSearch> searchService)
        {
            _entityFileTypeMimeTypeMapper = entityFileTypeMimeTypeMapper;
            _exportService = exportService;
            _mapper = mapper;
            _searchService = searchService;
        }

        public async Task<FileResult> ExportEntitiesAsync(EntityFileType fileType,
            string fileName,
            TSearch search,
            CancellationToken cancellationToken = default)
        {
            if (search == null)
            {
                throw new ArgumentNullException(nameof(search));
            }

            var entities = await _searchService
                .SearchAsync(search, cancellationToken)
                .ConfigureAwait(false);

            var records = MapRecords(entities);

            var stream = await _exportService
                .ExportAsync(fileType, records, cancellationToken)
                .ConfigureAwait(false);

            var mimeType = _entityFileTypeMimeTypeMapper.MapMimeType(fileType);
            var extension = _entityFileTypeMimeTypeMapper.GetExtension(fileType);

            var fileResult = new FileStreamResult(stream, mimeType)
            {
                FileDownloadName = $"{fileName}{extension}"
            };

            return fileResult;
        }

        private TMapped[] MapRecords(IEnumerable<TEntity> data)
        {
            if (data == null)
            {
                return new TMapped[0];
            }
            
            var mappedRecords = data
                .Select(_mapper.Map)
                .ToArray();

            return mappedRecords;
        }
    }
}