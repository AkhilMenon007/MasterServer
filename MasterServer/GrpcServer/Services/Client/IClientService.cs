namespace MasterServer.Services.Client
{
    /// <summary>
    /// Implement this interface to be automatically pulled by the DRCommunicator class
    /// </summary>
    public interface IClientService
    {
        void InitService(IDRCommunicator communicator);
        void EndService(IDRCommunicator communicator);
    }
}
