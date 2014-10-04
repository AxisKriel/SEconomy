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
using Wolfje.Plugins.SEconomy.Journal;

namespace Wolfje.Plugins.SEconomy {
	public static class ConsoleEx {
		static readonly object __consoleWriteLock = new object();

		public static void WriteBar(JournalLoadingPercentChangedEventArgs args)
		{
			StringBuilder output = new StringBuilder();
			int fillLen = 0;
			char filler = '#', spacer = ' ';

			output.Append(" ");
			for (int i = 0; i < 10; i++) {
				char c = i < args.Label.Length ? args.Label[i] : ' ';
				output.Append(c);
			}
			
			output.Append(" [");
			
			fillLen = Convert.ToInt32(((decimal)args.Percent / 100) * 60);

			for (int i = 0; i < 60; i++) {
				output.Append(i <= fillLen ? filler : spacer);
			}

			output.Append("] ");
			output.Append(args.Percent + "%");

			lock (__consoleWriteLock) {
				Console.Write("\r");
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.Write(output.ToString());
				Console.ResetColor();
			}
		}

		public static void WriteColour(ConsoleColor colour, string MessageFormat, params object[] args)
		{
			lock (__consoleWriteLock) {
				string s = string.Format(MessageFormat, args);

				try {
					ConsoleColor origColour = Console.ForegroundColor;
					Console.ForegroundColor = colour;
					Console.Write(s);
					Console.ForegroundColor = origColour;
				} catch {
					Console.Write(s);
				}
			}
		}

		public static void WriteLineColour(ConsoleColor colour, string MessageFormat, params object[] args)
		{
			lock (__consoleWriteLock) {
				string s = string.Format(MessageFormat, args);

				try {
					ConsoleColor origColour = Console.ForegroundColor;
					Console.ForegroundColor = colour;
					Console.WriteLine(s);
					Console.ForegroundColor = origColour;
				} catch {
					Console.WriteLine(s);
				}
			}
		}

		public static void WriteAtEnd(int Padding, ConsoleColor Colour, string MessageFormat, params object[] args)
		{
			lock (__consoleWriteLock) {
				string s = string.Format(MessageFormat, args);

				try {
					ConsoleColor origColour = Console.ForegroundColor;
					Console.ForegroundColor = Colour;
					Console.SetCursorPosition(Console.WindowWidth - s.Length - Padding, Console.CursorTop);
					Console.Write(s);
					Console.ForegroundColor = origColour;
				} catch {
					Console.Write(s);
				}
			}
		}

	}
}
