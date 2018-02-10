using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Ekklesia.Hubs
{
    internal class SongHub : Hub
    {
        public async Task ChangeSongPart(ChangeSongPartMessage message)
        {
            await Clients.All.InvokeAsync("Send", message);
        }

        public async Task Browse(BrowseMessage message)
        {
            await Clients.All.InvokeAsync("Send", message);
        }

        public async Task ControlVideo(VideoControlMessage message)
        {
            await Clients.All.InvokeAsync("Send", message);
        }
    }

    internal class ChangeSongPartMessage : MessageBase
    {
        protected ChangeSongPartMessage() : base("song:change_part")
        {
        }

        public string Id { get; set; }
        public string PartIdentifier { get; set; }
    }

    internal class BrowseMessage : MessageBase
    {
        protected BrowseMessage() : base("url:browse")
        {
        }

        public string Url { get; set; }
    }

    internal class VideoControlMessage : MessageBase
    {
        protected VideoControlMessage() : base("video:control")
        {
        }

        public string Action { get; set; }
    }

    internal class MessageBase
    {
        protected MessageBase(string messageType)
        {
            Type = messageType;
        }

        public string Type { get; set; }
    }
}
