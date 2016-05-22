﻿using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Web.Engine.Extensions
{
    public static class FormFileExtensions
    {
        /// <summary>
        ///     Parses content to get the file name of the uploaded file.
        /// </summary>
        /// <param name="formFile">The <see cref="IFormFile" />.</param>
        /// <returns></returns>
        public static string FileName(this IFormFile formFile)
        {
            return ContentDispositionHeaderValue.Parse(formFile.ContentDisposition)
                .FileName
                ?.Trim('"');
        }
    }
}