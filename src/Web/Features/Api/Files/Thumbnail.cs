﻿using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Web.Engine.Helpers;
using Web.Engine.Services;
using Web.Engine.Validation.Custom;
using Web.Models;

namespace Web.Features.Api.Files
{
    public class Thumbnail
    {
        public class Query : IAsyncRequest<Result>
        {
            public int? Id { get; set; }
        }

        public class QueryValidator : AbstractValidator<Query>
        {
            public QueryValidator(IDocumentSecurity documentSecurity)
            {
                RuleFor(m => m.Id)
                    .NotNull()
                    .HasDocumentFileAccess(documentSecurity, PermissionTypes.Read);
            }
        }

        public class QueryHandler : IAsyncRequestHandler<Query, Result>
        {
            private readonly ApplicationDbContext _db;
            private readonly IFileEncryptor _encryptor;

            public QueryHandler(ApplicationDbContext db, IFileEncryptor encryptor)
            {
                _db = db;
                _encryptor = encryptor;
            }

            public async Task<Result> Handle(Query message)
            {
                Debug.Assert(message.Id != null);

                var file = await _db.Files
                    .Where(f => f.Id == message.Id.Value)
                    .SingleOrDefaultAsync();

                if (file == null)
                {
                    return null;
                }

                var fileKey = _encryptor
                    .DecryptBase64(file.Key);

                var model = new Result
                {
                    FileContents = _encryptor
                        .DecryptFile(file.ThumbnailPath, fileKey)
                };

                return model;
            }
        }

        public class Result
        {
            public byte[] FileContents { get; set; }
            public string ContentType => "image/png";
        }
    }
}