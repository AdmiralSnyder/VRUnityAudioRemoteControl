//using Blazor.Extensions.Logging;
//using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace VrProjectWebsite
{
    public interface IHasLog
    {
        List<string> _messages { get; set; }
        List<string> LogOutput { get; set; }
    }
}
