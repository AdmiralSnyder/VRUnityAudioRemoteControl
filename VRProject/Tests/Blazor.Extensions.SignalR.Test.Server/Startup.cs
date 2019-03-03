using Blazor.Extensions.SignalR.Test.Server.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Blazor.Extensions.SignalR.Test.Server
{
    public partial class Startup
    {
        public void Configure_SignalRHub(IApplicationBuilder app, IHostingEnvironment env)
            => app.UseSignalR(routes => routes.MapHub<UnityRemoteControlHub>($"/{nameof(UnityRemoteControlHub).ToLower()}"));

        public void Configure_CustomMiddleware(IApplicationBuilder app, IHostingEnvironment env)
        {
            // eigene Middleware einbinden.
            string postedFolder = "posted";
            app.UseFileUpload(env, postedFolder, true);
            app.UseOggConverter(env, postedFolder);
            app.UseFileDownloader(env, postedFolder);
        }
    }
}
