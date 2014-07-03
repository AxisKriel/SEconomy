using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.InteropServices;

namespace Wolfje.Plugins.SEconomy.Packets {

    public enum FacingDirectionX : byte {
        Left = 0,
        Right = 1
    }
    public enum FacingDirectionY : byte {
        Up = 0,
        Down = 1
    }


    /// <summary>
    /// The strongly-typed Update NPC packet struct
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct UpdateNPC {
        public short NPCSlot;
        public float PositionX;
        public float PositionY;
        public float VelocityX;
        public float VelocityY;
        public short TargetPlayerID;
        public FacingDirectionX FacingDirectionX;
        public FacingDirectionY FacingDirectionY;
        public int Life;
        
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] int[] AI;

        public short Type;
    }

}
