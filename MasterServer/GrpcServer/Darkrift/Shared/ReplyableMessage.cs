using DarkRift;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MasterServer.Darkrift.Shared
{
    public class ReplyableMessage<T> : IDarkRiftSerializable where T : IDarkRiftSerializable,new()
    {
        public ushort messageID;
        public T serializable;

        public ReplyableMessage(ushort messageID, T serializable)
        {
            this.messageID = messageID;
            this.serializable = serializable;
        }

        public void Deserialize(DeserializeEvent e)
        {
            messageID = e.Reader.ReadUInt16();
            serializable = e.Reader.ReadSerializable<T>();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(messageID);
            e.Writer.Write(serializable);
        }
    }
}
