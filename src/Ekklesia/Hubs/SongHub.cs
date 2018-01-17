using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Ekklesia.Hubs
{
    internal class SongHub : Hub
    {
        public async Task Send(ToSend toSend)
        {
            await Clients.All.InvokeAsync("Send", toSend.Message);
        }
    }

    public class ToSend
    {
        public string Message { get; set; }
    }
}
