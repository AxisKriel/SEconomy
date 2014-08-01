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
