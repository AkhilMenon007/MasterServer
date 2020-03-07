using System.Threading.Tasks;
using MasterServer.Config;
using Grpc.Net.Client;
using MasterServer.DarkRift.Shared;
using MasterServer.Darkrift.Shared.Models;

namespace MasterServer.DarkRift
{
    public class DRClientManager
    {
        private readonly ServerAddresses config;
        private readonly DRCommunicator communicator;

        public DRClientManager(ServerAddresses config, DRCommunicator communicator)
        {
            this.config = config;
            this.communicator = communicator;
        }


        public async Task<ushort> UserLoggedIn(string userID)
        {
            var channel = GrpcChannel.ForAddress(config.UserServerAddress);
            var client = new UserService.Protos.UserManagement.UserManagementClient(channel);
            var res = await client.GetLoggedInUserAsync(new UserService.Protos.UserInfo { UserID = userID });

            if(string.IsNullOrEmpty(res.CharID)) 
            {
                System.Console.WriteLine("Failed to get logged in character from user server..");
                return 0;
            }

            var response = await communicator.SendMessageWithReply<ServerCharData,ServerAuthReply>(new ServerCharData {charID = res.CharID,jsonData = res.CharData }, (ushort)MasterServerReplyTags.CharacterLoggedIn);
            return response.sessToken;
        }
    }
}
