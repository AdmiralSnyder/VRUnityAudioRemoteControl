using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Xabe.FFmpeg;

namespace Blazor.Extensions.SignalR.Test.Server
{
    public static class FileDownloaderExtensions
    {
        /// <summary>
        /// Fügt die FileDownloader-Middleware hinzu. sie stellt Dateien in einem Unterordner zur Verfügung
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="env"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseFileDownloader(this IApplicationBuilder builder, IHostingEnvironment env, string folder)
        {
            FileDownloaderMiddleware.WorkPath = env.WebRootPath ?? env.ContentRootPath;
            FileDownloaderMiddleware.Folder = folder;
            return builder.UseMiddleware<FileDownloaderMiddleware>();
        }
    }

    public class FileDownloaderMiddleware
    {
        private readonly RequestDelegate _next;
        private IHostingEnvironment _hostingEnv;
        internal static string WorkPath;
        internal static string Folder;

        public FileDownloaderMiddleware(RequestDelegate next, IHostingEnvironment hostingEnv)
        {
            _next = next;
            _hostingEnv = hostingEnv;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Method == HttpMethods.Get)
            {
                string pathValue = context.Request.Path.Value;
                if (pathValue.Contains($"/{Folder}/"))
                {
                    string file = pathValue.Split($"/{Folder}/").Last();
                    string path = Path.Combine(WorkPath, Folder, file);
                    if (File.Exists(path))
                    {
                        context.Response.StatusCode = 200;
                        context.Response.ContentType = "audio/ogg";
                        await context.Response.SendFileAsync(path);
                    }
                    return;
                }
            }

            await _next.Invoke(context);
        }
    }
}
