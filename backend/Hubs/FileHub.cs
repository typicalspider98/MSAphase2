using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace backend.Hubs
{
    public class FileHub : Hub
    {
        public async Task NotifyFileUploaded(string fileName)
        {
            await Clients.All.SendAsync("FileUploaded", fileName);
        }

        public async Task NotifyFileDeleted(string fileName)
        {
            await Clients.All.SendAsync("FileDeleted", fileName);
        }
    }
}
