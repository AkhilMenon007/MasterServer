using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MasterServer.Services
{
    public class ClientInfo : IClientInfo
    {
        private Dictionary<string, string> userIDLookup;

        public ClientInfo()
        {
            userIDLookup = new Dictionary<string, string>();
        }
        public void AddClientInfo(string userName, string sessionKey)
        {
            userIDLookup.Add(sessionKey, userName);
        }
    }
}
