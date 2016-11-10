﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Web.Engine;
using Web.Engine.Helpers;
using Web.Engine.Validation.Custom;
using Web.Models;

namespace Web.Features.Api.Search
{
    public class Get
    {
        public class Query : IAsyncRequest<Result>
        {
            public int[] LibraryIds { get; set; } = {};

            public string Q { get; set; }

            public int PageIndex { get; set; }

            public string OrderBy { get; set; }

            public int MaxResults { get; set; } = Constants.SearchResultsPageSize;
        }

        public class QueryValidator : AbstractValidator<Query>
        {
            public QueryValidator(IDocumentSecurity documentSecurity)
            {
                RuleFor(m => m.MaxResults)
                    .InclusiveBetween(Constants.SearchResultsPageSize, Constants.SearchResultsMaxPageSize);
                RuleFor(m => m.LibraryIds)
                    .HasLibraryPermission(documentSecurity, PermissionTypes.Read);
            }
        }

        public class QueryHandler : IAsyncRequestHandler<Query, Result>
        {
            private readonly ApplicationDbContext _db;
            private readonly IConfigurationProvider _config;
            private readonly IDocumentSecurity _documentSecurity;

            public QueryHandler(ApplicationDbContext db,
                IConfigurationProvider config,
                IDocumentSecurity documentSecurity)
            {
                _db = db;
                _config = config;
                _documentSecurity = documentSecurity;
            }

            public async Task<Result> Handle(Query message)
            {
                var documentQuery = _db.Documents
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(message.Q))
                {
                    documentQuery = documentQuery
                        .FromSql(
                            $"SELECT d.* FROM [dbo].[{nameof(Document)}s] AS d JOIN FREETEXTTABLE([dbo].[vDocumentSearch], *, @p0) AS s ON d.Id = s.[Key] AND d.Status = @p1 AND EXISTS (SELECT 1 FROM [dbo].[{nameof(File)}s] AS f WHERE f.DocumentId = d.Id AND f.Status = @p1)",
                            message.Q, (int) StatusTypes.Active);
                }
                
                if (!message.LibraryIds.Any())
                {
                    // no libraries so default to all the user libraries

                    var userLibraryIds = await _documentSecurity
                        .GetUserLibraryIdsAsync(PermissionTypes.Read);

                    message.LibraryIds = userLibraryIds
                        .Select(i => i)
                        .ToArray();
                }

                // limit based on libraries the user can access

                documentQuery = documentQuery
                    .Where(dq => dq.Libraries.Any(l => message.LibraryIds.Contains(l.LibraryId)));

                var result = new Result
                {
                    TotalCount = await documentQuery
                        .CountAsync()
                };

                if (result.TotalCount > message.MaxResults * (message.PageIndex + 1))
                {
                    result.NextLink =
                        $"/api/search/?{nameof(message.Q)}={message.Q}{string.Join($"&{nameof(message.LibraryIds)}=", message.LibraryIds)}&{nameof(message.OrderBy)}={message.OrderBy}&{nameof(message.PageIndex)}={message.PageIndex + 1}";
                }

                result.Documents = await documentQuery
                    .Skip(message.MaxResults * message.PageIndex)
                    .Take(message.MaxResults)
                    .ProjectTo<Result.Document>(_config)
                    .ToArrayAsync();

                return result;
            }

            public class MappingProfile : Profile
            {
                public MappingProfile()
                {
                    CreateMap<Document, Result.Document>()
                        .ForMember(d => d.File, o => o.MapFrom(s =>
                            s.Files
                                .Where(f => f.Status == StatusTypes.Active)
                                .OrderByDescending(f => f.VersionNum)
                                .Single()));
                    CreateMap<File, Result.Document.FileResult>();
                }
            }
        }

        public class Result
        {
            public string NextLink { get; set; }
            public int TotalCount { get; set; }

            public IEnumerable<Document> Documents { get; set; }

            public class Document
            {
                public int Id { get; set; }
                public string Title { get; set; }
                public string SelfLink => $"/api/documents/{Id}";
                public string Abstract { get; set; }

                public FileResult File { get; set; }

                public class FileResult
                {
                    public int Id { get; set; }
                    public long Size { get; set; }
                    public int PageCount { get; set; }
                    public string ThumbnailLink => $"/api/files/{Id}/thumbnail";
                    public string ViewLink => $"/api/files/{Id}/view";
                }
            }
        }
    }
}