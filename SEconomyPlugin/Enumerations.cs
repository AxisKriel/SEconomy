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
using System.Text;

namespace Wolfje.Plugins.SEconomy {

	/// <summary>
	/// Player control flags for packet 0x0D - Player Control
	/// </summary>
	[Flags]
	public enum PlayerControlFlags : byte {
		/// <summary>
		/// Player is idle
		/// </summary>
		Idle = 0,
		/// <summary>
		/// Player has the down direction button pressed
		/// </summary>
		DownPressed = 1,
		/// <summary>
		/// Player has the left direction button pressed
		/// </summary>
		LeftPressed = 1 << 1,
		/// <summary>
		/// Player has the right direction button pressed
		/// </summary>
		RightPressed = 1 << 2,
		/// <summary>
		/// Player has the jump button pressed
		/// </summary>
		JumpPressed = 1 << 3,
		/// <summary>
		/// Player is pressing the "use" button
		/// </summary>
		UseItemPressed = 1 << 4,
		/// <summary>
		/// Direction the player is facing.  If 1 then player is facing right, if 0 then player is facing left.
		/// </summary>
		DirectionFacingRight = 1 << 5
	}

}
