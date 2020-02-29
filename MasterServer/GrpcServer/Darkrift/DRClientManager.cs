using MasterServer.DarkRift.Authentication;
using System.Threading.Tasks;
using MasterServer.Config;
using Grpc.Net.Client;
using MasterServer.DarkRift.Shared;
using MasterServer.Darkrift.Shared.Models;

namespace MasterServer.DarkRift
{
    public class DRClientManager
    {
        private readonly DRAuthenticator auth;
        private readonly ServerAddresses config;
        private readonly DRCommunicator communicator;

        public DRClientManager(DRAuthenticator auth,ServerAddresses config, DRCommunicator communicator)
        {
            this.auth = auth;
            this.config = config;
            this.communicator = communicator;
        }


        public async Task<ushort> UserLoggedIn(string userID)
        {
            if (!auth.authTask.IsCompletedSuccessfully) 
            {
                await auth.authTask;
            }
            if(!auth.authenticated)
            {
                System.Console.WriteLine("Masterserver Auth had failed..");
                return 0;
            }
            var channel = GrpcChannel.ForAddress(config.UserServerAddress);
            var client = new UserService.Protos.UserManagement.UserManagementClient(channel);
            var res = await client.GetLoggedInUserAsync(new UserService.Protos.UserInfo { UserID = userID });

            var response = await communicator.SendMessageWithReply<ServerCharData,ServerAuthReply>(new ServerCharData {charID = res.CharID,jsonData = res.CharData }, (ushort)MasterServerReplyTags.CharacterLoggedIn);
            return response.sessToken;
        }
    }
}
