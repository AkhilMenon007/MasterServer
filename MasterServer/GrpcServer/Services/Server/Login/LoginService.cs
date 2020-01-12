using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using LoginAPI.GameServer;
using MasterServer.Services;
using DarkRift;
using DarkRift.Client;
using DarkRiftHelpers;

namespace MasterServer
{
    public class LoginService : GameServerAuth.GameServerAuthBase
    {
        private readonly IClientInfo _clientInfo;
        private readonly IDRCommunicator _drCommunicator;

        private const string CONNECTION_STRING = "Connecting..";

        public LoginService(ILogger<LoginService> logger,IClientInfo clientInfo,IDRCommunicator drCommunicator)
        {
            _clientInfo = clientInfo;
            _drCommunicator = drCommunicator;

            Init();
        }

        private void Init()
        {
        }

        public async override Task<ServerReply> SayHello(UserReg request, ServerCallContext context)
        {        

            _clientInfo.AddClientInfo(request.UserID, request.SessionToken);

            Console.WriteLine($"{request.UserID} has requested login");
            await _drCommunicator.SendMessage(new StringContainer() { val = CONNECTION_STRING});
            try 
            {
                return await Task.FromResult(new ServerReply()
                {
                    ReplyMessage = 0
                });
            }
            catch 
            {
                Console.WriteLine("Error from Master server or Darkrift Server..");
                return new ServerReply { ReplyMessage = -1};
            }
        }
    }
}
