namespace MasterServer.Services
{
    public interface IClientInfo
    {
        void AddClientInfo(string userName, string sessionKey);
    }
}