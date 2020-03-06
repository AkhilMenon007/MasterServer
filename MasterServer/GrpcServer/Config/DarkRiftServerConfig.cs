using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MasterServer.Config
{
    public class DarkRiftServerConfig
    {
        public string IP { get; set; }
        public string PassCode { get; set; } = "Tpu3pHh5/dXrZZaghUwcz7kxEiVO1yuNHrDC7bu2J4A=";
        public int AuthRequestTries { get; set; } = 10;
        public int AuthRequestDelay { get; set; } = 60000;

        public byte[] SecretKeyArray => Convert.FromBase64String(PassCode);
    }
}
