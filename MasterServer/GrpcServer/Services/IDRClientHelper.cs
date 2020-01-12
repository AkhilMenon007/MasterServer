using DarkRift.Client;

namespace MasterServer.Services
{
    public interface IDRClientHelper
    {
        DarkRiftClient GenerateDarkriftClient();
    }
}