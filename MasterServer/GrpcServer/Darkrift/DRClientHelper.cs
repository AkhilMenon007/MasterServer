using DarkRift;
using DarkRift.Client;
using MasterServer.Config;
using System;
using System.Net;

namespace MasterServer.DarkRift
{
    public class DRClientHelper
    {

        public string connectionAddress = "";

        private DarkRiftClient _client = null;
        public DRClientHelper(DarkRiftServerConfig config) 
        {
            //connectionAddress = configuration.GetValue<string>("AllowedHosts");
            connectionAddress = config.IP;
        }

        public DarkRiftClient GetDarkriftClient()
        {
            var client = new DarkRiftClient();
            while (_client == null)
            {
                var ep = IPEndPoint.Parse(connectionAddress);

                try
                {
                    client.Connect(ep.Address, ep.Port, IPVersion.IPv4);
                    _client = client;
                    return client;
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            return _client;
        }

    }
}
