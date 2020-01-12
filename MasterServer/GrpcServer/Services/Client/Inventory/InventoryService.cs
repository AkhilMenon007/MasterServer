using Grpc.Net.Client;
using InventoryService.Protos;
using MasterServer.DarkRiftHelpers.Tags;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MasterServer.Services.Client.Inventory
{
    public class InventoryService : IInventoryService,IClientService
    {
        private string inventoryServerAddress = "";
        public void InitService(IDRCommunicator communicator)
        {

            inventoryServerAddress = communicator.configuration.GetValue<string>("InventoryServerIP");
            communicator.Client.MessageReceived += ClientResponse;
        }

        private async void ClientResponse(object sender, DarkRift.Client.MessageReceivedEventArgs e)
        {
            if (e.Tag == Tags.GetMessageTag(Tags.masterServiceID,MasterServerTags.authenticate)) 
            {
                using (var channel = GrpcChannel.ForAddress(inventoryServerAddress))
                {
                    var client = new InventoryManagement.InventoryManagementClient(channel);
                    try 
                    {
                        await client.DropItemAsync(new InventoryItem());
                    }
                    catch 
                    {
                        Console.WriteLine("Error from inventory service");
                    }
                }
            }

        }

        public void EndService(IDRCommunicator communicator)
        {
            communicator.Client.MessageReceived -= ClientResponse;
        }
    }
}
