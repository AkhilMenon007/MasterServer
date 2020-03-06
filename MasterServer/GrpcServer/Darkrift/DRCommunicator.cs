using DarkRift;
using DarkRift.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MasterServer.Config;
using MasterServer.DarkRift.Shared;
using MasterServer.Darkrift;

namespace MasterServer.DarkRift
{
    public class DRCommunicator
    {
        /// <summary>
        /// The messages from game server to the master server will be working on request reply protocol like fashion
        /// When a message is sent it will have a message id value associated with it, which the server echoes back.
        /// </summary>
        private readonly Dictionary<ushort,TaskCompletionSource<byte[]>> waitingOnReply = 
            new Dictionary<ushort, TaskCompletionSource<byte[]>>();

        /// <summary>
        /// The Amount of messages sent, can also be used as the id of next message to be sent
        /// </summary>
        private int messageCount = 0;
        public DRClientHelper helper { get; }

        private DarkRiftClient Client => helper.GetDarkriftClient();

        public DRCommunicator(DRClientHelper helper)
        {
            this.helper = helper;

            Init();
        }

        private void Init()
        {
            Console.WriteLine("Connecting to server");
            Client.MessageReceived += SignalReply;
        }

        private ushort GetMessageID()
        {
            ushort messID;
            if (messageCount > ushort.MaxValue - 10)
            {
                messID = (ushort)Interlocked.Exchange(ref messageCount, 0);
            }
            else
            {
                messID = (ushort)Interlocked.Increment(ref messageCount);
            }

            return messID;
        }

        public async Task<U> SendMessageWithReply<T, U>(T serializable, ushort tag) where T : IDarkRiftSerializable, new() where U : IDarkRiftSerializable, new()
        {
            using (var writer = DarkRiftWriter.Create())
            {
                ushort messID = GetMessageID();

                var token = new TaskCompletionSource<byte[]>();

                writer.Write(messID);
                writer.Write<IDarkRiftSerializable>(serializable);
                using (var message = Message.Create(tag, writer))
                {
                    Console.WriteLine("Sending message to DR2 Server..");
                    Client.SendMessage(message, SendMode.Reliable);
                }
                waitingOnReply.Add(messID, token);
                Console.WriteLine("Sent message waiting on Reply..");

                await Task.Yield();

                Task.WaitAny(Task.Delay(100000),token.Task);
                waitingOnReply.Remove(messID);
                if (token.Task.IsCompletedSuccessfully)
                {
                    if (token.Task.Result.Length != 0)
                    {
                        return DarkRiftExtensions.ReadBytesOfSerializable<U>(token.Task.Result);
                    }
                }
                return default;
            }
        }

        private void SignalReply(object sender, MessageReceivedEventArgs e)
        {
            if (TagManagement.GetService(e.Tag) != TagManagement.masterServiceReply)
            {
                return;
            }
            using (var message = e.GetMessage()) 
            {
                using(var reader = message.GetReader()) 
                {
                    ushort id = reader.ReadUInt16();
                    if (waitingOnReply.TryGetValue(id, out TaskCompletionSource<byte[]> src))
                    {
                        src.SetResult(reader.ReadRaw(reader.Length - reader.Position));
                    }
                }
            }
        }
    }
}
