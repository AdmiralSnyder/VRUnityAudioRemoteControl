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
    public static class OggConverterExtensions
    {
        public static IApplicationBuilder UseOggConverter(this IApplicationBuilder builder, IHostingEnvironment env)
        {
            OggConverterMiddleware.WorkPath = env.WebRootPath ?? env.ContentRootPath;
            Xabe.FFmpeg.FFmpeg.ExecutablesPath = Path.Combine(OggConverterMiddleware.WorkPath, "ffmpeg");
            Directory.CreateDirectory(Xabe.FFmpeg.FFmpeg.ExecutablesPath);
            Xabe.FFmpeg.FFmpeg.GetLatestVersion();
            return builder.UseMiddleware<OggConverterMiddleware>();
        }
    }

    public class OggConverterMiddleware
    {
        private readonly RequestDelegate _next;
        private IHostingEnvironment _hostingEnv;
        internal static string WorkPath;

        public OggConverterMiddleware(RequestDelegate next, IHostingEnvironment hostingEnv)
        {
            _next = next;
            _hostingEnv = hostingEnv;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Method == HttpMethods.Post
                && context.Items.TryGetValue("SavedFiles", out var savedFilesObj)
                && savedFilesObj is List<string> savedFiles)
            {
                foreach (var savedFile in savedFiles.Where(sf => sf.EndsWith(".mp3", StringComparison.InvariantCultureIgnoreCase) || sf.EndsWith(".wav", StringComparison.InvariantCultureIgnoreCase)))
                {
                    await ConvertToOggAsync(Path.Combine(WorkPath, savedFile));
                    await context.Response.WriteAsync($"Successfully converted.{Environment.NewLine}{$"{savedFile}.ogg"}");
                }
            }
            else
            {
                await _next.Invoke(context);
            }
        }

        private async Task ConvertToOggAsync(string savedFile)
        {
            await Conversion.Convert(savedFile, savedFile + ".ogg").Start();
        }
    }
}
