using System.Threading.Tasks;
using Ekklesia.Songs;
using Microsoft.AspNetCore.SignalR;

namespace Ekklesia.Hubs
{
    internal class SongHub : Hub
    {
        public async Task ChangeSongPart(ChangeSongPartMessage message)
        {
            await Clients.All.InvokeAsync("Send", message);
        }
    }

    internal class ChangeSongPartMessage : MessageBase
    {
        protected ChangeSongPartMessage() : base("song:change_part")
        {
        }

        public SongPart SongPart { get; set; }
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
