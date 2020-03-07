using System;

namespace MasterServer.DarkRift
{
    public partial class DRClientHelper
    {
        [Serializable]
        public class DRAuthException : Exception
        {
            public DRAuthException() { }
            public DRAuthException(string message) : base(message) { }
            public DRAuthException(string message, Exception inner) : base(message, inner) { }
            protected DRAuthException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }
    }
}
