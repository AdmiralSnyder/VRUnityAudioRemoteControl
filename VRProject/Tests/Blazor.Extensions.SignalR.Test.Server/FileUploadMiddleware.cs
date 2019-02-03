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
        public static IApplicationBuilder UseFileUpload(this IApplicationBuilder builder, IHostingEnvironment env, bool successPassthrough = false)
        {
            Directory.CreateDirectory(Path.Combine(env.WebRootPath ?? env.ContentRootPath, FileUploadMiddleware.PostedFolderName));
            FileUploadMiddleware.SuccessPassthrough = successPassthrough;
            return builder.UseMiddleware<FileUploadMiddleware>();
        }
    }

    public class FileUploadMiddleware
    {
        public const string PostedFolderName = "posted";
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
                if (files.Any())
                {
                    var savedFiles = new List<string>();
                    context.Items.Add("SavedFiles", savedFiles);

                    foreach (var file in files)
                    {
                        var extension = Path.GetExtension(file.FileName);
                        var guid = Guid.NewGuid().ToString();
                        var filename = Path.Combine(PostedFolderName, guid + extension);
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
                await context.Response.WriteAsync("Successfully uploaded.");
                if (SuccessPassthrough && success)
                {
                    await _next.Invoke(context);
                }
                //x.Files.First().OpenReadStream().CopyToAsync()
                //try
                //{
                //    if (context.Request.Body.CanRead && context.Request.ContentLength.HasValue)
                //    {
                //        string body = new StreamReader(context.Request.Body).ReadToEnd();

                //        //var len = (int)context.Request.ContentLength.Value;
                //        //byte[] bytes = new byte[len];
                //        //var read = context.Request.Body.Read(bytes, 0, len);

                //        //string body = new StreamReader(context.Request.Body).ReadToEnd();

                //        //(int)context.Request.ContentLength
                //        //var mstr = new MemoryStream(bytes);
                //        //mstr.Position = 0;

                //        //await context.Request.Body.CopyToAsync(mstr);
                //        //byte b = bytes[7];
                //        //byte c = bytes[8];
                //    }
                //    var path = context.Request.Query["ImageFile"];
                //    if (string.IsNullOrEmpty(path))
                //    {
                //        var form = context.Request.Form;
                //        path = context.Request.Form["ImageFile"];
                //    }
                //    if (string.IsNullOrEmpty(path))
                //    {
                //        path = "File";
                //    }

                //    var files = context.Request.Form.Files;
                //    List<string> filePathList = new List<string>();
                //    List<string> realPathList = new List<string>();
                //    foreach (var file in files)
                //    {
                //        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                //        string filePath = Path.Combine(path, fileName);
                //        string fileDir = Path.Combine(_hostingEnv.WebRootPath, path);
                //        if (!Directory.Exists(fileDir))
                //        {
                //            Directory.CreateDirectory(fileDir);
                //        }
                //        string realPath = Path.Combine(_hostingEnv.WebRootPath, filePath);

                //        filePathList.Add(filePath);
                //        realPathList.Add(realPath);

                //        using (FileStream fs = System.IO.File.Create(realPath))
                //        {
                //            file.CopyTo(fs);
                //            fs.Flush();
                //        }
                //    }
                //    context.Response.StatusCode = 200;
                //    await context.Response.WriteAsync("Successfully uploaded.");
                //}
                //catch (Exception e)
                //{
                //    await context.Response.WriteAsync("Upload errors");
                //}
            }
            else
            {
                await _next.Invoke(context);
            }
        }
    }
}
