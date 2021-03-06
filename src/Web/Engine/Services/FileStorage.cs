﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Web.Models;
using File = System.IO.File;

namespace Web.Engine.Services
{
    public interface IFileStorage
    {
        Task<(string Key, string IV, string Path)> Save(Stream stream);
        Task<string> Save(Stream stream, string key, string iv);

        Task Delete(string path);
        void TryDelete(params string[] paths);

        FileMeta Open(string path, string extension, string key, string iv);
        Task<FileMeta> Open(int id);
        Task<FileMeta> OpenThumbnail(int id);
    }

    public class FileStorage : IFileStorage
    {
        private readonly ApplicationDbContext _db;
        private readonly string _storageRoot;
        private readonly IFileEncryptor _encryptor;
        private readonly IFileDecoder _fileDecoder;

        public FileStorage(IOptions<DRSConfig> config, ApplicationDbContext db, IFileEncryptor encryptor, IFileDecoder fileDecoder)
        {
            if (config.Value == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = db ?? throw new ArgumentNullException(nameof(db));
            _encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor));
            _fileDecoder = fileDecoder ?? throw new ArgumentNullException(nameof(fileDecoder));
            _storageRoot = config.Value.DocumentPath ?? throw new ArgumentNullException(nameof(config.Value.DocumentPath));
        }

        private (string Key, string IV) EncryptAccessKeys(byte[] key, byte[] iv)
            => (_encryptor.Encrypt(key), _encryptor.Encrypt(iv));

        private (byte[] Key, byte[] IV) DecryptAccessKeys(string key, string iv)
            => (_encryptor.DecryptBase64(key), _encryptor.DecryptBase64(iv));

        private async Task<int> GetNextDirectorySeed()
        {
            var any = await _db.Documents
                .AnyAsync()
                .ConfigureAwait(false);

            if (!any)
            {
                return 1;
            }

            var max = await _db.Documents
                .MaxAsync(d => d.Id)
                .ConfigureAwait(false);

            return ++max;
        }

        private FileMeta Open(string path, string extension, byte[] key, byte[] iv)
        {
            Stream streamCreator() => _encryptor.Decrypt(File.OpenRead(path), key, iv);
            var contentType = MimeTypes.MimeTypeMap.GetMimeType(extension);

            return new FileMeta(streamCreator, contentType, _fileDecoder.Get(extension));
        }

        private IQueryable<DataFile> GetDataFile(int id)
            => _db.DataFiles.Where(df => df.Id == id);

        public async Task<FileMeta> Open(int id)
        {
            var dataFile = await GetDataFile(id)
                .Select(df => new
                {
                    df.Path,
                    df.Extension,
                    df.Key,
                    df.IV
                })
                .SingleAsync()
                .ConfigureAwait(false);

            return Open(dataFile.Path, dataFile.Extension, dataFile.Key, dataFile.IV);
        }

        public async Task<FileMeta> OpenThumbnail(int id)
        {
            var dataFile = await GetDataFile(id)
                .Select(df => new
                {
                    Path = df.ThumbnailPath,
                    df.Key,
                    df.IV
                })
                .SingleAsync()
                .ConfigureAwait(false);

            return Open(dataFile.Path, ".png", dataFile.Key, dataFile.IV);
        }

        public FileMeta Open(string path, string extension, string key, string iv)
        {
            var (Key, IV) = DecryptAccessKeys(key, iv);

            return Open(path, extension, Key, IV);
        }

        public async Task<(string Key, string IV, string Path)> Save(Stream stream)
        {
            var accessKeys = _encryptor.GenerateKeyAndIv();

            var path = await Save(stream, accessKeys.Key, accessKeys.IV)
                .ConfigureAwait(false);

            var (Key, IV) = EncryptAccessKeys(accessKeys.Key, accessKeys.IV);

            return (Key, IV, path);
        }

        public Task<string> Save(Stream stream, string key, string iv)
        {
            var (Key, IV) = DecryptAccessKeys(key, iv);

            return Save(stream, Key, IV);
        }

        private async Task<string> Save(Stream stream, byte[] key, byte[] iv)
        {
            var path = await GetNewFileName()
                .ConfigureAwait(false);

            await CreateDirectoryIfNotFound(Path.GetDirectoryName(path))
                .ConfigureAwait(false);

            using (var encrypted = _encryptor.Encrypt(stream, key, iv))
            {
                using (var output = File.Create(path))
                {
                    encrypted.CopyTo(output);
                    encrypted.Dispose();
                }
            }

            return path;
        }

        public void TryDelete(params string[] paths)
        {
            foreach (var path in paths)
            {
                try
                {
                    Delete(path)
                        .ConfigureAwait(false);
                }
                catch
                {
                }
            }
        }

        public async Task Delete(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            await Task.Factory
                .StartNew(() => File.Delete(path))
                .ConfigureAwait(false);
        }

        private async Task<string> GetNewFileName()
            => GetNewFileName(await GetNextDirectorySeed().ConfigureAwait(false), _storageRoot);

        private static string GetNewFileName(int directorySeed, string rootPath)
        {
            var subFolder1 = Math.Floor(directorySeed / 1024m / 1024m / 1024m);
            var subFolder2 = Math.Floor(subFolder1 / 1024m / 1024m);
            var subFolder3 = Math.Floor(subFolder2 / 1024m);

            return Path.Combine(rootPath,
                $@"{subFolder1}\{subFolder2}\{subFolder3}\{Guid.NewGuid():N}.bin");
        }

        private Task CreateDirectoryIfNotFound(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                return Task.FromResult(0);
            }

            return Task.Factory
                .StartNew(() => Directory.CreateDirectory(dirPath));
        }
    }
}