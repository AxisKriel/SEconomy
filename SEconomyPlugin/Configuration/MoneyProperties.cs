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
	public class MoneyProperties {
		public bool UseQuadrantNotation = true;
		public string MoneyName = "coin";
		public string MoneyNamePlural = "coins";
		public string SingularDisplayFormat = "N0";
		public string SingularDisplayCulture = "en-US";

		public string Quadrant1FullName = "Copper";
		public string Quadrant1ShortName = "copper";
		public string Quadrant1Abbreviation = "c";

		public string Quadrant2FullName = "Silver";
		public string Quadrant2ShortName = "silver";
		public string Quadrant2Abbreviation = "s";

		public string Quadrant3FullName = "Gold";
		public string Quadrant3ShortName = "gold";
		public string Quadrant3Abbreviation = "g";

		public string Quadrant4FullName = "Platinum";
		public string Quadrant4ShortName = "plat";
		public string Quadrant4Abbreviation = "p";
	}
}
