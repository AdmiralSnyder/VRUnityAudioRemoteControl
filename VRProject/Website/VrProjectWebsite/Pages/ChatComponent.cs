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

namespace VrProjectWebsite//Blazor.Extensions.SignalR.Test.Client.Pages
{
    public class ILogger<T>
    {
        internal void LogInformation(string v, string v1)
        {
            //throw new NotImplementedException("LOGGER FEHLT");
        }

        internal void LogInformation(object v)
        {
            LogMessages.Add(v.ToString());
            //throw new NotImplementedException("LOGGER FEHLT");
        }

        internal void LogError(Exception exc, string v)
        {
            LogMessages.Add("Error " + v + " : " + exc.ToString());
        }

        public List<string> LogMessages { get; set; }
    }

    public class ChatComponent : BlazorComponent
    {
        [Inject] private HttpClient _http { get; set; }
        /*[Inject] */
        private ILogger<ChatComponent> _logger { get; set; } = new ILogger<ChatComponent>();
        internal string _toEverybody { get; set; }

        private string __Color;

        internal string _Color {
            get => __Color;
            set
            {
                __Color = value;
                SetColor(__Color);
            }
        }
        internal string _toConnection { get; set; }
        internal string _connectionId { get; set; }
        internal string _toMe { get; set; }
        internal string _toGroup { get; set; }
        internal string _groupName { get; set; }
        internal List<string> _messages { get; set; } = new List<string>();

        internal List<string> LogOutput { get; set; } = new List<string>();

        private IDisposable _objectHandle;
        private IDisposable _listHandle;
        private IDisposable _multiArgsHandle;
        private IDisposable _multiArgsComplexHandle;
        private IDisposable _byteArrayHandle;
        private HubConnection _connection;

        protected override async Task OnInitAsync()
        {
            _logger.LogMessages = LogOutput;
            _connection = new HubConnectionBuilder()
                .WithUrl("/chathub",
                opt =>
                {
                    opt.LogLevel = SignalRLogLevel.None;
                    opt.Transport = HttpTransportType.WebSockets;
                    opt.SkipNegotiation = true;
                    opt.AccessTokenProvider = async () =>
                    {
                        var token = await GetJwtToken("DemoUser");
                        _logger.LogInformation($"Access Token: {token}");
                        return token;
                    };
                })
                .AddMessagePackProtocol()
                .Build();

            _connection.On<string, string, string>("Send", Handle);
            _connection.On<string>("Send", Handle);
            _connection.OnClose(exc =>
            {
                _logger.LogError(exc, "Connection was closed!");
                return Task.CompletedTask;
            });
            await _connection.StartAsync();
        }

        public Task DemoMethodObject(object data)
        {
            _logger.LogInformation("Got object!");
            _logger.LogInformation(data?.GetType().FullName ?? "<NULL>");
            _objectHandle.Dispose();
            if (data == null) return Task.CompletedTask;
            return Handle(data);
        }

        public Task DemoMethodList(object data)
        {
            _logger.LogInformation("Got List!");
            _logger.LogInformation(data?.GetType().FullName ?? "<NULL>");
            _listHandle.Dispose();
            if (data == null) return Task.CompletedTask;
            return Handle(data);
        }

        public Task DemoMultipleArgs(string arg1, int arg2, string arg3, int arg4)
        {
            _logger.LogInformation("Got Multiple Args!");
            _multiArgsHandle.Dispose();

            return HandleArgs(arg1, arg2, arg3, arg4);
        }

        public Task DemoMultipleArgsComplex(object arg1, object arg2)
        {
            _logger.LogInformation("Got Multiple Args Complex!");
            _multiArgsComplexHandle.Dispose();

            return HandleArgs(arg1, arg2);
        }

        public Task DemoByteArrayArg(byte[] array)
        {
            _logger.LogInformation("Got byte array!");
            _byteArrayHandle.Dispose();

            return HandleArgs(BitConverter.ToString(array));
        }

        private async Task<string> GetJwtToken(string userId)
        {
            var httpResponse = await _http.GetAsync($"/generatetoken?user={userId}");
            httpResponse.EnsureSuccessStatusCode();
            return await httpResponse.Content.ReadAsStringAsync();
        }

        private Task Handle(object msg)
        {
            _logger.LogInformation(msg);
            _messages.Add(msg.ToString());
            StateHasChanged();
            return Task.CompletedTask;
        }

        private Task Handle(string connectionID, string userIdentifier, string message)
        {
            _logger.LogInformation($"[{connectionID}] {userIdentifier}: {message}");
            _messages.Add($"{userIdentifier}: {message}");
            StateHasChanged();
            return Task.CompletedTask;
        }

        private Task HandleArgs(params object[] args)
        {
            string msg = string.Join(", ", args);

            _logger.LogInformation(msg);
            _messages.Add(msg);
            StateHasChanged();
            return Task.CompletedTask;
        }

        internal async Task Broadcast()
        {
            _logger.LogInformation("BROADCASTBROADCASTBROADCASTBROADCASTBROADCASTBROADCAST3");
            await _connection.InvokeAsync("Send", _toEverybody);
        }

        internal void SetColor_NOTBOUND(UIChangeEventArgs e)
        {
            
            _logger.LogInformation("SEEEEEEETTTTTCCCCCCCCCCCOOOOOOOOOOOOLLLLLLLLLLLOOOOOOOOOOOORRRRRRRRR3");
            Debug.WriteLine("SEEEEEEETTTTTCCCCCCCCCCCOOOOOOOOOOOOLLLLLLLLLLLOOOOOOOOOOOORRRRRRRRR");
            Console.WriteLine("SEEEEEEETTTTTCCCCCCCCCCCOOOOOOOOOOOOLLLLLLLLLLLOOOOOOOOOOOORRRRRRRRR21");
            _connection.InvokeAsync("Farbe", e.Value.ToString());
        }

        internal async Task SetColor(string value)
        {
            _logger.LogInformation("YYYYSEEEEEEETTTTTCCCCCCCCCCCOOOOOOOOOOOOLLLLLLLLLLLOOOOOOOOOOOORRRRRRRRR3");
            Debug.WriteLine("XXXXXXSEEEEEEETTTTTCCCCCCCCCCCOOOOOOOOOOOOLLLLLLLLLLLOOOOOOOOOOOORRRRRRRRR");
            Console.WriteLine("BBBBSEEEEEEETTTTTCCCCCCCCCCCOOOOOOOOOOOOLLLLLLLLLLLOOOOOOOOOOOORRRRRRRRR21");
            await _connection.InvokeAsync("Farbe", value);
        }

        internal async Task SendToOthers()
        {
            await _connection.InvokeAsync("SendToOthers", _toEverybody);
        }

        internal async Task SendToConnection()
        {
            await _connection.InvokeAsync("SendToConnection", _connectionId, _toConnection);
        }

        internal async Task SendToMe()
        {
            await _connection.InvokeAsync("Echo", _toMe);
        }

        internal async Task SendToGroup()
        {
            await _connection.InvokeAsync("SendToGroup", _groupName, _toGroup);
        }

        internal async Task SendToOthersInGroup()
        {
            await _connection.InvokeAsync("SendToOthersInGroup", _groupName, _toGroup);
        }

        internal async Task JoinGroup()
        {
            await _connection.InvokeAsync("JoinGroup", _groupName);
        }

        internal async Task LeaveGroup()
        {
            await _connection.InvokeAsync("LeaveGroup", _groupName);
        }

        internal async Task DoMultipleArgs()
        {
            _multiArgsHandle = _connection.On<string, int, string, int>("DemoMultiArgs", DemoMultipleArgs);
            _multiArgsComplexHandle = _connection.On<DemoData, DemoData[]>("DemoMultiArgs2", DemoMultipleArgsComplex);
            await _connection.InvokeAsync("DoMultipleArgs");
            await _connection.InvokeAsync("DoMultipleArgsComplex");
        }

        internal async Task DoByteArrayArg()
        {
            _byteArrayHandle = _connection.On<byte[]>("DemoByteArrayArg", DemoByteArrayArg);
            var array = await _connection.InvokeAsync<byte[]>("DoByteArrayArg");

            _logger.LogInformation("Got byte returned from hub method array: {0}", BitConverter.ToString(array));
        }
        internal ElementRef fileInput;
        internal string FileUploadContent { get; set; }
        
        internal async Task UploadFile()
        {
            _logger.LogInformation("UploadFileUploadFileUploadFileUploadFileUploadFile");
            var fileName = await JSRuntime.Current.InvokeAsync<string>("getFileName", fileInput);
            var base64File = await JSRuntime.Current.InvokeAsync<string>("getFileData", fileInput);
            _logger.LogInformation("GetFileData: after " + base64File.Length + " " + base64File.Substring(0, 20));
            var guid = Guid.NewGuid();
            var maxsize = 32000;
            var parts = (int)Math.Ceiling((double)base64File.Length / maxsize);
            foreach (var (part, idx) in SplitBy(base64File, maxsize).Select((s, i) => (s, i)))
            {
                await _connection.InvokeAsync("File", fileName, guid.ToString(), parts, idx, part);
            }

            _logger.LogInformation("sent");
        }

        public static IEnumerable<string> SplitBy(string str, int chunkLength)
        {
            if (string.IsNullOrEmpty(str)) throw new ArgumentException();
            if (chunkLength < 1) throw new ArgumentException();

            for (int i = 0; i < str.Length; i += chunkLength)
            {
                if (chunkLength + i > str.Length)
                {
                    chunkLength = str.Length - i;
                }

                yield return str.Substring(i, chunkLength);
            }
        }

        internal async Task TellHubToDoStuff()
        {
            _objectHandle = _connection.On<DemoData>("DemoMethodObject", DemoMethodObject);
            _listHandle = _connection.On<DemoData[]>("DemoMethodList", DemoMethodList);
            await _connection.InvokeAsync("DoSomething");
        }
    }
}
