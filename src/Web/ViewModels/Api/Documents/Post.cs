﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.AspNet.Http;
using Microsoft.Data.Entity;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Extensions.PlatformAbstractions;
using Web.Engine;
using Web.Engine.Codecs.Decoders;
using Web.Engine.Extensions;
using Web.Models;

namespace Web.ViewModels.Api.Documents
{
    public class Post
    {
        public class Command : IAsyncRequest<int?>
        {
            public IFormFile File { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(m => m.File)
                    .NotNull();
            }
        }

        public class CommandHandler : IAsyncRequestHandler<Command, int?>
        {
            private readonly ApplicationDbContext _db;
            private readonly IOptions<DRSSettings> _settings;
            private readonly IUserAccessor _userAccessor;
            private readonly IApplicationEnvironment _appEnvironment;

            public CommandHandler(ApplicationDbContext db, IOptions<DRSSettings> settings, IUserAccessor userAccessor, IApplicationEnvironment appEnvironment)
            {
                _appEnvironment = appEnvironment;
                _db = db;
                _settings = settings;
                _userAccessor = userAccessor;
            }

            public async Task<int?> Handle(Command message)
            {
                const DataProtectionScope dataProtectionScope = DataProtectionScope.LocalMachine;

                var extension = Path.GetExtension(message.File.FileName() ?? "")
                    .ToLowerInvariant();

                var documentKey = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N"));
                
                var document = new Document
                {
                    CreatedByUserId = _userAccessor.User.GetUserId(),
                    CreatedOn = DateTimeOffset.Now,
                    Extension = extension,
                    FileSize = message.File.Length,
                    ModifiedOn = DateTimeOffset.Now,
                    ThumbnailPath = "",
                    Title = message.File.FileName(),
                    PageCount = 0,
                    Path = "",
                    Key = Convert.ToBase64String(documentKey.Protect(null, dataProtectionScope))
                };

                _db.Documents.Add(document);

                //todo: add to indexers private library (hardcoded for now)

                document.Libraries.Add(await _db.Libraries
                    .Select(l => new LibraryDocument
                    {
                        Library = l
                    })
                    .FirstAsync());

                using (var trans = await _db.Database.BeginTransactionAsync())
                {
                    await _db.SaveChangesAsync();

                    // generate the document paths

                    var destPath = GetNewFileName(_settings.Value.DocumentDirectory, document.Id);

                    Debug.Assert(destPath != null);

                    if (!Directory.Exists(Path.GetDirectoryName(destPath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                    }

                    // write the file to the drive

                    var buffer = message.File.OpenReadStream()
                        .ToByteArray();

                    // get a parser

                    var parser = Engine.Codecs.Decoders.File.Get(extension, buffer, _appEnvironment);

                    // index in lucene

                    var fileContents = await parser.ContentAsync();

                    document.Abstract = fileContents?.NormalizeLineEndings()?.Truncate(512);

                    document.Content = new DocumentContent
                    {
                        Content = fileContents
                    };

                    // update the paths on the record

                    document.Path = destPath;
                    document.ThumbnailPath = $"{destPath}{Constants.ThumbnailExtension}";

                    try
                    {
                        // thumbnail

                        using (var stream = new MemoryStream())
                        {
                            // save the thumbnail to the stream

                            await parser.ThumbnailAsync(stream, 600);

                            // encrypt the stream and save it

                            await stream.ToArray()
                                .SaveProtectedAsAsync(document.ThumbnailPath, documentKey, dataProtectionScope);
                        }

                        document.PageCount = await parser.PageCountAsync();

                        // encrypt and save the uploaded file

                        await buffer.SaveProtectedAsAsync(document.Path, documentKey, dataProtectionScope);

                        // save and commit

                        await _db.SaveChangesAsync();
                    }
                    catch
                    {
                        // some clean ups

                        // thumbnail

                        if (System.IO.File.Exists(document.ThumbnailPath))
                        {
                            System.IO.File.Delete(document.ThumbnailPath);
                        }

                        // document

                        if (System.IO.File.Exists(document.Path))
                        {
                            System.IO.File.Delete(document.Path);
                        }

                        trans.Rollback();

                        throw;
                    }

                    trans.Commit();

                    // the document that was created

                    return document.Id;
                }
            }

            private static string GetNewFileName(string rootPath, int seed)
            {
                var subFolder1 = ((seed / 1024) / 1024);
                var subFolder2 = ((seed / 1024) - (((seed / 1024) / 1024) * 1024));

                return Path.Combine(rootPath,
                    $@"{subFolder1}\{subFolder2}\{Guid.NewGuid():N}.bin");
            }
        }
    }
}