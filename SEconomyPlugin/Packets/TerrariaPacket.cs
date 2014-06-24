using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.InteropServices;

namespace Wolfje.Plugins.SEconomy.Packets {
    [StructLayout(LayoutKind.Sequential)]
    public struct TerrariaPacket {
        public int Length;
        byte MessageType;
        
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 65526)]
        public byte[] MessagePayload;

        public PacketTypes PacketType {
            get {
                return (PacketTypes)MessageType;
            }
        }
    }
}
