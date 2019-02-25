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
        public static IApplicationBuilder UseFileDownloader(this IApplicationBuilder builder, IHostingEnvironment env)
        {
            FileDownloaderMiddleware.WorkPath = env.WebRootPath ?? env.ContentRootPath;
            return builder.UseMiddleware<FileDownloaderMiddleware>();
        }
    }

    public class FileDownloaderMiddleware
    {
        private readonly RequestDelegate _next;
        private IHostingEnvironment _hostingEnv;
        internal static string WorkPath;

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
                if (pathValue.Contains("/posted/"))
                {
                    string file = pathValue.Split("/posted/").Last();
                    string path = Path.Combine(WorkPath, "posted", file);
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
