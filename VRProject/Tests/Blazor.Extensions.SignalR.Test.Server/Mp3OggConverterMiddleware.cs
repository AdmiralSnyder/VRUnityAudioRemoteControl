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
    public static class Mp3OggConverterExtensions
    {
        public static IApplicationBuilder UseMp3OggConverter(this IApplicationBuilder builder, IHostingEnvironment env)
        {
            Mp3OggConverterMiddleware.WorkPath = env.WebRootPath ?? env.ContentRootPath;
            Xabe.FFmpeg.FFmpeg.ExecutablesPath = Path.Combine(Mp3OggConverterMiddleware.WorkPath, "ffmpeg");
            Directory.CreateDirectory(Xabe.FFmpeg.FFmpeg.ExecutablesPath);
            Xabe.FFmpeg.FFmpeg.GetLatestVersion();
            return builder.UseMiddleware<Mp3OggConverterMiddleware>();
        }
    }

    public class Mp3OggConverterMiddleware
    {
        private readonly RequestDelegate _next;
        private IHostingEnvironment _hostingEnv;
        internal static string WorkPath;

        public Mp3OggConverterMiddleware(RequestDelegate next, IHostingEnvironment hostingEnv)
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
                foreach (var savedFile in savedFiles.Where(sf => sf.EndsWith(".mp3", StringComparison.InvariantCultureIgnoreCase)))
                {
                    await ConvertMp3ToOggAsync(Path.Combine(WorkPath, savedFile));
                }
            }
            else
            {
                await _next.Invoke(context);
            }
        }

        private async Task ConvertMp3ToOggAsync(string savedFile)
        {
            await Conversion.Convert(savedFile, savedFile + ".ogg").Start();
        }
    }
}
