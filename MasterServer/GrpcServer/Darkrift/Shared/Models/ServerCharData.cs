using DarkRift;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MasterServer.DarkRift.Shared
{
    public class ServerCharData : IDarkRiftSerializable
    {
        public string charID { get; set; }
        public string jsonData { get; set; }
        public void Deserialize(DeserializeEvent e)
        {
            charID = e.Reader.ReadString();
            jsonData = e.Reader.ReadString();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(charID);
            e.Writer.Write(jsonData);
        }
    }
}
