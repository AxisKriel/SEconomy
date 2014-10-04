/*
 * This file is part of SEconomy - A server-sided currency implementation
 * Copyright (C) 2013-2014, Tyler Watson <tyler@tw.id.au>
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */
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
