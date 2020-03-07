using DarkRift;
using DarkRift.Client;
using MasterServer.Config;
using MasterServer.DarkRift.Shared;
using System;
using System.Net;
using System.Threading.Tasks;

namespace MasterServer.DarkRift
{
    public partial class DRClientHelper
    {
        public EventHandler<MessageReceivedEventArgs> OnClientMessageReceived { get; set; }
        public EventHandler<DisconnectedEventArgs> OnClientDisconnected { get; set; }
        public Action<DarkRiftClient> OnClientConnected { get; set; }

        private readonly string connectionAddress;
        private readonly DarkRiftServerConfig config;

        private TaskCompletionSource<ushort> authSuccessToken = null;
        private bool authenticated = false;

        private DarkRiftClient client = null;

        public DRClientHelper(DarkRiftServerConfig config)
        {
            connectionAddress = config.IP;
            this.config = config;
            Init();
        }

        private void Init()
        {
            OnClientDisconnected += ClientDisconnected;
        }

        public async Task<DarkRiftClient> GetDarkriftClient()
        {
            if (this.client != null) 
            {
                return this.client;
            }
            var client = GetNewClient();
            if (client != null)
            {
                await StartAuth(client);
                if (authenticated) 
                {
                    this.client = client;
                    OnClientConnected?.Invoke(client);
                    client.MessageReceived += OnClientMessageReceived;
                    client.Disconnected += OnClientDisconnected;
                    return client;
                }
            }
            return null;
        }

        private void ClientDisconnected(object sender, DisconnectedEventArgs e)
        {
            client = null;
            authenticated = false;
        }

        private DarkRiftClient GetNewClient()
        {
            if (this.client == null)
            {
                var client = new DarkRiftClient();
                var ep = IPEndPoint.Parse(connectionAddress);

                try
                {
                    client.Connect(ep.Address, ep.Port, IPVersion.IPv4);
                    if (client.ConnectionState == ConnectionState.Connected)
                    {
                        this.client = client;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            return this.client;
        }

        private async Task StartAuth(DarkRiftClient client)
        {
            for (int i = 0; i < config.AuthRequestTries; i++)
            {
                authenticated = false;
                try
                {
                    var res = await Authenticate(client);
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
        public async Task<bool> Authenticate(DarkRiftClient client)
        {
            authSuccessToken = new TaskCompletionSource<ushort>();
            using (var writer = DarkRiftWriter.Create())
            {
                using (var message = Message.Create((ushort)MasterServerNoReplyTags.AuthRequest, writer))
                {
                    client.SendMessage(message, SendMode.Reliable);
                    client.MessageReceived += SendPassword;
                    await Task.Yield();
                    Task.WaitAny(Task.Delay(config.AuthRequestDelay), authSuccessToken.Task);
                    if (authSuccessToken.Task.IsCompletedSuccessfully)
                    {
                        if (authSuccessToken.Task.Result != (ushort)MasterServerAuthReplies.Success)
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
            var client = (DarkRiftClient)sender;
            if (e.Tag != (ushort)MasterServerNoReplyTags.PublicKey)
            {
                return;
            }
            client.MessageReceived -= SendPassword;
            using (var message = e.GetMessage())
            {
                using (var reader = message.GetReader().DecryptReaderAES(config.SecretKeyArray))
                {
                    var publicKey = Convert.FromBase64String(reader.ReadString());
                    var commonKey = reader.ReadString();
                    using (var writer = DarkRiftWriter.Create())
                    {
                        writer.Write(commonKey);
                        using (var encryptedWriter = writer.EncryptWriterAES(publicKey))
                        {
                            using (var reply = Message.Create((ushort)MasterServerNoReplyTags.Password, encryptedWriter))
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
            var client = (DarkRiftClient)sender;
            if (e.Tag != (ushort)MasterServerNoReplyTags.Acknowledge)
            {
                return;
            }
            client.MessageReceived -= AcknowledgeAuth;
            using (var message = e.GetMessage())
            {
                using (var reader = message.GetReader())
                {
                    var res = reader.ReadUInt16();
                    authSuccessToken.SetResult(res);
                }
            }
        }
    }
}
