using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MasterServer.DarkRift.Shared
{
    public static class TagManagement
    {
        public const ushort playerServiceID = 0;
        public const ushort masterServiceNoReply = 1;
        public const ushort masterServiceReply = 2;



        public static ushort GetService(ushort input)
        {
            return (ushort)(input >> 8);
        }
        public static ushort GetTag(ushort input)
        {
            return (ushort)(input % (1 << 8));
        }
        public static ushort GetMessageTag(ushort serviceTag, ushort messageTag)
        {
            return (ushort)((serviceTag << 8) + messageTag);
        }
    }
}
