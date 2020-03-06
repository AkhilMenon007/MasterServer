using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MasterServer.ClientShared.Models
{
    public class ClientAuthResponse
    {
        public ushort sessionToken { get; set; } = 0;
    }
}
