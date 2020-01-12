using DarkRift;
using DarkRift.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MasterServer.Services
{
    public class DRClientHelper : IDRClientHelper
    {

        public string connectionAddress = "";

        public DRClientHelper(Config.DarkRiftServerConfig config) 
        {
            //connectionAddress = configuration.GetValue<string>("AllowedHosts");
            connectionAddress = config.IP;
        }

        public DarkRiftClient GenerateDarkriftClient()
        {
            var client = new DarkRiftClient();
            var ep = IPEndPoint.Parse(connectionAddress);
            try 
            {
                client.Connect(ep.Address, ep.Port, IPVersion.IPv4);
                return client;
            }
            catch 
            {
                return null;
            }
        }

    }
}
