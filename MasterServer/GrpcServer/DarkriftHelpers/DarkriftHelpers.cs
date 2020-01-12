using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DarkRift;

namespace DarkRiftHelpers
{

    public class IntegerContainer : IDarkRiftSerializable
    {
        public int val;
        public void Deserialize(DeserializeEvent e)
        {
            val = e.Reader.ReadUInt16();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(val);
        }
    }
    public class StringContainer : IDarkRiftSerializable 
    {
        public string val;

        public void Deserialize(DeserializeEvent e)
        {
            val = e.Reader.ReadString();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(val);
        }
    }


    public class DarkriftHelpers
    {

    }
}
