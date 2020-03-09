using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MasterServer.DarkRift.Shared
{
    public enum MasterServerNoReplyTags
    {
        AuthRequest = TagManagement.masterServiceNoReply<<8,
        PublicKey,
        Password,
        Acknowledge,
        SaveCharacterData
    }
}
