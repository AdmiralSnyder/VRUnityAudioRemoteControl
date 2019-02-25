using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Blazor.Extensions.SignalR.Test.Server
{
    public static class FileUploadExtensions
    {
        public static IApplicationBuilder UseFileUpload(this IApplicationBuilder builder, IHostingEnvironment env, string folder, bool successPassthrough = false)
        {
            Directory.CreateDirectory(Path.Combine(env.WebRootPath ?? env.ContentRootPath, FileUploadMiddleware.Folder));
            FileUploadMiddleware.SuccessPassthrough = successPassthrough;
            FileUploadMiddleware.Folder = folder;
            return builder.UseMiddleware<FileUploadMiddleware>();
        }
    }

    public class FileUploadMiddleware
    {
        internal static string Folder { get; set; } = "posted";
        internal static bool SuccessPassthrough { get; set; }

        private readonly RequestDelegate _next;
        private IHostingEnvironment _hostingEnv;

        public FileUploadMiddleware(RequestDelegate next, IHostingEnvironment hostingEnv)
        {
            _next = next;
            _hostingEnv = hostingEnv;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Method == HttpMethods.Post)
            {
                var forms = await context.Request.ReadFormAsync();
                var files = forms.Files;
                bool success = false;
                var savedFiles = new List<string>();
                if (files.Any())
                {
                    context.Items.Add("SavedFiles", savedFiles);
                    foreach (var file in files)
                    {
                        var extension = Path.GetExtension(file.FileName);
                        var guid = Guid.NewGuid().ToString();
                        var filename = Path.Combine(Folder, guid + extension);
                        using (var outstream = File.Create(filename))
                        {
                            await file.CopyToAsync(outstream);
                        
                            outstream.Flush();
                        }
                        savedFiles.Add(filename);
                        success = true;
                    }
                }

                context.Response.StatusCode = 200;
                await context.Response.WriteAsync($"  ");

                if (SuccessPassthrough && success)
                {
                    await _next.Invoke(context);
                }
            }
            else
            {
                await _next.Invoke(context);
            }
        }
    }
}
