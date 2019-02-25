//using Blazor.Extensions.Logging;
using Blazor.Extensions;
using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.JSInterop;
//using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VrProjectWebsite.Shared;

namespace VrProjectWebsite
{
    public class ChatComponent : BlazorComponent
    {
        [Inject]
        private HttpClient Http { get; set; }
        private HubConnection Connection;

        /* [Inject] */
        private ILogger<ChatComponent> Logger { get; set; } = new ILogger<ChatComponent>(); 

        private async Task<string> GetJwtToken(string userId)
        {
            var httpResponse = await Http.GetAsync($"/generatetoken?user={userId}");
            httpResponse.EnsureSuccessStatusCode();
            return await httpResponse.Content.ReadAsStringAsync();
        }

        internal string CommandText { get; set; }

        internal string Color {
            get => _Color;
            set
            {
                _Color = value;
                SetColor(_Color);
            }
        }
        private string _Color;

        internal string Animation
        {
            get => _Animation;
            set
            {
                _Animation = value;
                SetAnimation(_Animation);
            }
        }
        private string _Animation;

        public HashSet<string> Animations = new HashSet<string>();

        public List<string> LogOutput { get; set; } = new List<string>();

        protected override async Task OnInitAsync()
        {
            Logger.LogMessages = LogOutput;

            //TheChatLayout.CascadingParameterValue = "gesetzt in OnInitAsync";

            Connection = new HubConnectionBuilder()
                .WithUrl("/chathub",
                opt =>
                {
                    opt.LogLevel = SignalRLogLevel.None;
                    opt.Transport = HttpTransportType.WebSockets;
                    opt.SkipNegotiation = true;
                    opt.AccessTokenProvider = async () =>
                    {
                        var token = await GetJwtToken("DemoUser");
                        Logger.LogInformation($"Access Token: {token}");
                        return token;
                    };
                })
                .AddMessagePackProtocol()
                .Build();

            //only register for logging purposes
            Connection.On<string>("Command", Handle);

            Connection.On<string>("AddAnimations", animations =>
            {
                foreach(var animation in animations.Split(','))
                {
                    Animations.Add(animation);
                }
                StateHasChanged();
                return Task.CompletedTask;
            });

            Connection.OnClose(exc =>
            {
                Logger.LogError(exc, "Connection was closed!");
                return Task.CompletedTask;
            });

            await Connection.StartAsync();
        }

        private Task Handle(object msg)
        {
            Logger.LogInformation(msg);
            StateHasChanged(); 
            return Task.CompletedTask;
        }

        internal async Task BroadcastCommand() => await Connection.InvokeAsync("Command", CommandText);

        internal async Task SetColor(string color) => await Connection.InvokeAsync("Color", color);

        internal async Task SetAudio(string audio) => await Connection.InvokeAsync("Audio", audio);
        internal async Task SetAudioMute() => await Connection.InvokeAsync("Audio", "mute");
        internal async Task SetAudioUnmute() => await Connection.InvokeAsync("Audio", "unmute");
        internal async Task StopAudio() => await Connection.InvokeAsync("Audio", "stop");

        internal async Task SetAnimation(string animation) => await Connection.InvokeAsync("Animation", animation);
    }
}
