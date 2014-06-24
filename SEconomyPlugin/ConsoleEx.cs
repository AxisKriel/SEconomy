using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolfje.Plugins.SEconomy {
    public static class ConsoleEx {
        static readonly object __consoleWriteLock = new object();

        public static void WriteAtEnd(int Padding, ConsoleColor Colour, string MessageFormat, params object[] args) {
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
