using DarkRift;
using DarkRift.Client;
using MasterServer.Config;
using MasterServer.DarkRift.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MasterServer.DarkRift.Authentication
{
    public class DRAuthenticator
    {
        private readonly DarkRiftServerConfig config;


        private DarkRiftClient client = null;

        private TaskCompletionSource<ushort> authSuccessToken = null;
        public bool authenticated { get; set; }

        public Task authTask { get; set; }

        public DRAuthenticator(DRClientHelper clientHelper, DarkRiftServerConfig config)
        {
            client = clientHelper.GetDarkriftClient();
            this.config = config;
            authTask = Init();
        }

        private async Task Init()
        {
            authenticated = false;
            for (int i = 0; i < config.AuthRequestTries; i++)
            {
                try
                {
                    var res = await Authenticate();
                    if (res)
                    {
                        authenticated = true;
                        break;
                    }
                    if (i == config.AuthRequestTries - 1)
                    {
                        throw new Exception("Authentication failed multiple times");
                    }
                }
                catch (DRAuthException e)
                {
                    Console.WriteLine(e.ToString());
                    break;
                }
            }
        }
        public async Task<bool> Authenticate()
        {
            authSuccessToken = new TaskCompletionSource<ushort>();
            using(var writer = DarkRiftWriter.Create()) 
            {
                using(var message = Message.Create((ushort)MasterServerNoReplyTags.AuthRequest, writer)) 
                {
                    client.SendMessage(message, SendMode.Reliable);
                    client.MessageReceived += SendPassword;
                    await Task.Yield();
                    Task.WaitAny(Task.Delay(config.AuthRequestDelay), authSuccessToken.Task);
                    if (authSuccessToken.Task.IsCompletedSuccessfully) 
                    {
                        if(authSuccessToken.Task.Result != (ushort)MasterServerAuthReplies.Success) 
                        {
                            throw new DRAuthException("Authentication failed");
                        }
                        return authSuccessToken.Task.Result == (ushort)MasterServerAuthReplies.Success;
                    }
                    else 
                    {
                        authSuccessToken = null;
                        throw new DRAuthException("Authentication Timed Out..");
                    }
                }
            }
        }

        private void SendPassword(object sender, MessageReceivedEventArgs e)
        {
            if (e.Tag != (ushort)MasterServerNoReplyTags.PublicKey) 
            {
                return;
            }
            client.MessageReceived -= SendPassword;
            using(var message = e.GetMessage()) 
            {
                using(var reader = message.GetReader().DecryptReaderAES(config.SecretKeyArray)) 
                {
                    var publicKey = Convert.FromBase64String(reader.ReadString());
                    var commonKey = reader.ReadString();
                    using(var writer = DarkRiftWriter.Create()) 
                    {
                        writer.Write(commonKey);
                        using(var encryptedWriter = writer.EncryptWriterAES(publicKey)) 
                        {
                            using(var reply = Message.Create((ushort)MasterServerNoReplyTags.Password, encryptedWriter)) 
                            {
                                client.SendMessage(reply, SendMode.Reliable);
                                client.MessageReceived += AcknowledgeAuth;
                            }
                        }
                    }
                }
            }
        }

        private void AcknowledgeAuth(object sender, MessageReceivedEventArgs e)
        {
            if(e.Tag != (ushort)MasterServerNoReplyTags.Acknowledge) 
            {
                return;
            }
            client.MessageReceived -= AcknowledgeAuth;
            using (var message = e.GetMessage()) 
            {
                using(var reader = message.GetReader()) 
                {
                    var res = reader.ReadUInt16();
                    authSuccessToken.SetResult(res);
                }
            }
        }
    }

    [Serializable]
    public class DRAuthException : Exception
    {
        public DRAuthException() { }
        public DRAuthException(string message) : base(message) { }
        public DRAuthException(string message, Exception inner) : base(message, inner) { }
        protected DRAuthException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
