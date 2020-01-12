using DarkRift;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace MasterServer.Services
{
    public interface IDRCommunicator
    {
        DarkRift.Client.DarkRiftClient Client { get; }
        IConfiguration configuration { get; }
        Task SendMessage(IDarkRiftSerializable message);
    }
}