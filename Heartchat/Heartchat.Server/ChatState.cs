using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Heartchat.Server
{
    class ChatState
    {
        public Socket Socket = null;

        public const int BufferSize = 1024;

        public byte[] Buffer = new byte[1024];
        public string Data = string.Empty;

    }
}
