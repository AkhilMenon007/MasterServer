﻿using System.Threading.Tasks;
using MasterServer.Config;
using Grpc.Net.Client;
using MasterServer.DarkRift.Shared;
using MasterServer.Darkrift.Shared.Models;
using DarkRift.Client;
using System;

namespace MasterServer.DarkRift
{
    public class DRClientManager
    {
        private readonly ServerAddresses config;
        private readonly DRCommunicator communicator;
        private readonly DRClientHelper helper;

        public DRClientManager(ServerAddresses config, DRCommunicator communicator, DRClientHelper helper)
        {
            this.config = config;
            this.communicator = communicator;
            this.helper = helper;
            Init();
        }

        private void Init() 
        {
            helper.OnClientMessageReceived += HandleClientMessage;
        }

        private void HandleClientMessage(object sender, MessageReceivedEventArgs e)
        {
            switch ((MasterServerNoReplyTags)e.Tag)
            {
                case MasterServerNoReplyTags.SaveCharacterData: 
                    {
                        using (var message = e.GetMessage())
                        {
                            using(var reader = message.GetReader()) 
                            {
                                var mess = reader.ReadSerializable<ServerCharData>();
                                var channel = GrpcChannel.ForAddress(config.UserServerAddress);
                                var client = new UserService.Protos.UserManagement.UserManagementClient(channel);
                                var charInfo = new UserService.Protos.CharacterInfo { CharID = mess.charID, CharData = mess.jsonData };
                                client.SaveUserDataAsync(charInfo);
                            }
                        }
                        break;
                    }
            }
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

            var response = await communicator.SendMessageWithReply<ServerCharData,ServerAuthReply>(
                new ServerCharData {charID = res.CharID,jsonData = res.CharData }, 
                (ushort)MasterServerReplyTags.CharacterLoggedIn);
            return response.sessToken;
        }
    }
}
