using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MasterServer.Config
{
    public class DarkRiftServerConfig
    {
        public string IP { get; set; }
        public string SecretKey { get; set; } = "ARAARARA";
        public int AuthRequestTries { get; set; } = 10;
        public int AuthRequestDelay { get; set; } = 60000;
    }
}
