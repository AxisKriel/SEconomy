using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Wolfje.Plugins.SEconomy.Packets {
    /// <summary>
    /// The NPC strike packet 0x1C
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct DamageNPC {
        [MarshalAs(UnmanagedType.I2)]
        public short NPCID;
        [MarshalAs(UnmanagedType.I2)]
        public short Damage;
        [MarshalAs(UnmanagedType.R4)]
        public float Knockback;
        [MarshalAs(UnmanagedType.I1)]
        public byte Direction;
        [MarshalAs(UnmanagedType.I1)]
        public byte CrititcalHit;
    }
}
