using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MasterServer.DarkRiftHelpers.Tags
{
    static class Tags
    {
        public static ushort playerServiceID = 0;
        public static ushort masterServiceID = 1;



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
