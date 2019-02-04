using Microsoft.AspNetCore.Blazor.Builder;
//using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
//using Microsoft.AspNetCore.Http.Connections;
//using VrProjectWebsite.Hubs;
using System;
//using Blazor.Extensions;
using System.Threading.Tasks;
//using Microsoft.AspNetCore.SignalR;
//using Microsoft.AspNetCore.Routing;
//using Microsoft.AspNetCore.Http.Connections.Internal;

namespace VrProjectWebsite
{
    public class ComponentLayoutBindingService
    {

    }

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddTransient<>
            //services.AddLogging(builder => builder.AddBrowserConsole());

            //services.AddSignalR();
        }

        public void Configure(IBlazorApplicationBuilder app)
        {
            app.AddComponent<App>("app");
            
            //var connection = new HubConnectionBuilder()
            //    .WithUrl("/myHub", // The hub URL. If the Hub is hosted on the server where the blazor is hosted, you can just use the relative path.
            //    opt =>
            //    {
            //        opt.LogLevel = SignalRLogLevel.Trace; // Client log level
            //        opt.Transport = HttpTransportType.WebSockets; // Which transport you want to use for this connection
            //    })
            //    .Build(); // Build the HubConnection

            //connection.On<string>("Receive", this.Handle); // Subscribe to messages sent from the Hub to the "Receive" method by passing a handle (Func<object, Task>) to process messages.
            //await connection.StartAsync(); // Start the connection.

            //await connection.InvokeAsync("ServerMethod", param1, param2, paramX); // Invoke a method on the server called "ServerMethod" and pass parameters to it. 

            //var result = await connection.InvokeAsync<MyResult>("ServerMethod", param1, param2, paramX); // Invoke a method on the server called "ServerMethod", pass parameters to it and get the result back.

            #region delete
            /*if (app.Services.GetService<SignalRMarkerService>() == null)
            {
                throw new InvalidOperationException("Unable to find the required services. Please add all the required services by calling 'IServiceCollection.AddSignalR' inside the call to 'ConfigureServices(...)' in the application startup code.");
            }
            app.UseConnections(delegate (ConnectionsRouteBuilder routes)
            {
                configure(new HubRouteBuilder(routes));
            });
            return app;

            IApplicationBuilder ab;
            ab.UseConnections(delegate (ConnectionsRouteBuilder routes)
            {
                configure(new HubRouteBuilder(routes));
            });
            ab.UseSignalR();
            
            app.UseSignalR(routes =>
            {
                routes.MapHub<ChatHub>("/chatHub");
            });*/

            #endregion


        }

        //public Task Handle(object o)
        //{
        //    return Task.CompletedTask;
        //}
    }

    /*
    public static class ApplicationBuilderHelperStuff
    {
        /// <summary>
        /// Adds support for ASP.NET Core Connection Handlers to the <see cref="T:Microsoft.AspNetCore.Builder.IApplicationBuilder" /> request execution pipeline.
        /// </summary>
        /// <param name="app">The <see cref="T:Microsoft.AspNetCore.Builder.IApplicationBuilder" />.</param>
        /// <param name="configure">A callback to configure connection routes.</param>
        /// <returns>The same instance of the <see cref="T:Microsoft.AspNetCore.Builder.IApplicationBuilder" /> for chaining.</returns>
        public static IBlazorApplicationBuilder UseConnections(this IBlazorApplicationBuilder app, Action<ConnectionsRouteBuilder> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException("configure");
            }
            HttpConnectionDispatcher requiredService = app.Services.GetRequiredService<HttpConnectionDispatcher>();
            RouteBuilder routeBuilder = new RouteBuilder(app);
            configure(new ConnectionsRouteBuilder(routeBuilder, requiredService));
            app.UseWebSockets();
            app.UseRouter(routeBuilder.Build());
            return app;
        }

        /// <summary>
        /// Adds SignalR to the <see cref="T:Microsoft.AspNetCore.Builder.IApplicationBuilder" /> request execution pipeline.
        /// </summary>
        /// <param name="app">The <see cref="T:Microsoft.AspNetCore.Builder.IApplicationBuilder" />.</param>
        /// <param name="configure">A callback to configure hub routes.</param>
        /// <returns>The same instance of the <see cref="T:Microsoft.AspNetCore.Builder.IApplicationBuilder" /> for chaining.</returns>
        public static IApplicationBuilder UseSignalR(this IApplicationBuilder app, Action<HubRouteBuilder> configure)
        {
            if (app.ApplicationServices.GetService<SignalRMarkerService>() == null)
            {
                throw new InvalidOperationException("Unable to find the required services. Please add all the required services by calling 'IServiceCollection.AddSignalR' inside the call to 'ConfigureServices(...)' in the application startup code.");
            }
            app.UseConnections(delegate (ConnectionsRouteBuilder routes)
            {
                configure(new HubRouteBuilder(routes));
            });
            return app;
        }
    }
        */
}
