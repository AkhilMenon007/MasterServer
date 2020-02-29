using DarkRift;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MasterServer.Darkrift.Shared.Models
{
    public class ServerAuthReply : IDarkRiftSerializable
    {
        public ushort sessToken { get; set; }
        public void Deserialize(DeserializeEvent e)
        {
            sessToken = e.Reader.ReadUInt16();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(sessToken);
        }
    }
}
