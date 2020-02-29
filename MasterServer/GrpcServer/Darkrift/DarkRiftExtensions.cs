using DarkRift;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MasterServer.Darkrift
{
    public static class DarkRiftExtensions
    {
        public static T ReadBytesOfSerializable<T>(byte[] byteArray) where T : IDarkRiftSerializable,new()
        {
            using (var writer = DarkRiftWriter.Create()) 
            {
                writer.WriteRaw(byteArray,0,byteArray.Length);
                using(var message = Message.Create(0,writer)) 
                {
                    using(var reader = message.GetReader()) 
                    {
                        return reader.ReadSerializable<T>();
                    }
                }
            }
        }
    }
}
