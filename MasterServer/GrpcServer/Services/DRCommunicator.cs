using DarkRift;
using DarkRift.Client;
using MasterServer.Services.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MasterServer.Services
{
    public class DRCommunicator : IDRCommunicator
    {
        /// <summary>
        /// The messages from game server to the master server will be working on request reply protocol like fashion
        /// When a message is sent it will have a message id value associated with it, which the server echoes back.
        /// </summary>
        private Dictionary<ushort,TaskCompletionSource<int>> waitingOnReply;

        /// <summary>
        /// The Amount of messages sent, can also be used as the id of next message to be sent
        /// </summary>
        private int messageCount = 0;

        private static readonly ushort MESSAGE_TAG = DarkRiftHelpers.Tags.Tags.GetMessageTag(DarkRiftHelpers.Tags.Tags.masterServiceID, DarkRiftHelpers.Tags.MasterServerTags.acknowledge);
        private static readonly ushort AUTHENTICATE_TAG = DarkRiftHelpers.Tags.Tags.GetMessageTag(DarkRiftHelpers.Tags.Tags.masterServiceID, DarkRiftHelpers.Tags.MasterServerTags.authenticate);

        public DarkRiftClient Client { get; private set; }

        public IConfiguration configuration { get; private set; }

        private List<IClientService> clientServices;


        public DRCommunicator(IDRClientHelper helper,IConfiguration configuration)
        {
            this.configuration = configuration;

            Client = helper.GenerateDarkriftClient();
            waitingOnReply = new Dictionary<ushort, TaskCompletionSource<int>>();

            Init();
        }

        private async void Init()
        {
            Console.WriteLine("Connecting to server");
            Client.MessageReceived += SignalReply;
            clientServices = ClientServiceManager.RetrieveServices();
            foreach (var service in clientServices)
            {
                service.InitService(this);
            }
            int res;
            do
            {
                res = await AuthenticateToDRServer(Client);
            }
            while (res != 0);
        }


        public async Task<int> AuthenticateToDRServer(DarkRiftClient client) 
        {
            using (var writer = DarkRiftWriter.Create())
            {
                ushort messID = (ushort)Interlocked.Increment(ref messageCount);
                writer.Write(messID);

                writer.Write("Connecting...");

                var token = new TaskCompletionSource<int>();
                waitingOnReply.Add(messID, token);
                using (var _message = Message.Create(AUTHENTICATE_TAG, writer))
                {
                    Console.WriteLine(AUTHENTICATE_TAG);
                    Console.WriteLine("Authenticating to DR2 Server..");
                    Client.SendMessage(_message, SendMode.Reliable);
                }
                Console.WriteLine("Sent message waiting on Reply..");

                await Task.Yield();
                Task.WaitAny(Task.Delay(5000), token.Task);
                if (token.Task.IsCompleted) 
                {
                    if(token.Task.Result == 0) 
                    {
                        Console.WriteLine("Successfully authenticated with the DR2 Server..");
                        return 0;
                    }
                    else 
                    {
                        Console.WriteLine("Authentication failed with DR2 Server");
                        return 2;
                    }
                }
                else
                {
                    Console.WriteLine("Authentication Timed Out..");
                    return 1;
                }
            }
        }



        public async Task SendMessage(IDarkRiftSerializable message)
        {
            using (var writer = DarkRiftWriter.Create())
            {
                ushort messID = (ushort)Interlocked.Increment(ref messageCount);
                writer.Write(messID);

                var token = new TaskCompletionSource<int>();
                waitingOnReply.Add(messID, token);
                writer.Write<IDarkRiftSerializable>(message);
                using (var _message = Message.Create(MESSAGE_TAG, writer))
                {
                    Console.WriteLine("Sending message to DR2 Server..");
                    Client.SendMessage(_message, SendMode.Reliable);
                }
                Console.WriteLine("Sent message waiting on Reply..");

                await Task.Yield();

                Task.WaitAny(Task.Delay(5000),token.Task);
            }
        }

        private void SignalReply(object sender, MessageReceivedEventArgs e)
        {
            if (DarkRiftHelpers.Tags.Tags.GetService(e.Tag) != DarkRiftHelpers.Tags.Tags.masterServiceID)
            {
                return;
            }
            using (var message = e.GetMessage()) 
            {
                using(var reader = message.GetReader()) 
                {
                    ushort id = reader.ReadUInt16();
                    TaskCompletionSource<int> src;
                    if(waitingOnReply.TryGetValue(id,out src))
                    {
                        src.SetResult(0);
                        waitingOnReply.Remove(id);
                    }
                }
            }

        }
        ~DRCommunicator() 
        {
            foreach (var service in clientServices)
            {
                service.EndService(this);
            }
            Client.Disconnect();
        }
    }
}
